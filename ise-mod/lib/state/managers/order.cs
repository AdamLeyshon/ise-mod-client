#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, order.cs, Created 2021-02-26

#endregion

using System;
using System.Linq;
using ise.components;
using ise_core.db;
using Order;
using RestSharp;
using Verse;
using static ise.lib.Constants;
using static RimWorld.GenDate;
using static ise.lib.Tradables;
using static ise_core.rest.api.v1.Constants;
using static ise_core.rest.Helpers;
using static ise.lib.Crypto;

namespace ise.lib.state.managers
{
    internal class Order
    {
        #region Fields

        private const int TicksPerHalfDay = TicksPerDay / 2;
        private const int TicksUntilDelivery = TicksPerHour * 5;

        internal string OrderId { get; }
        private readonly DBOrder backingOrder;

        #endregion

        #region ctor

        internal Order(string orderId)
        {
            OrderId = orderId;
            var db = IseCentral.DataCache;
            backingOrder = db.GetCollection<DBOrder>(Tables.Orders).FindById(this.OrderId);
            Status = backingOrder.Status;
            Logging.WriteDebugMessage($"Order tracker initialised for {OrderId}");
        }

        #endregion

        #region Properties

        internal OrderStatusEnum Status { get; private set; }
        internal bool Busy { get; private set; }
        internal bool IsFinished { get; private set; }

        #endregion

        #region Methods

        internal void Update()
        {
            Busy = true;
            Status = backingOrder.Status;
            var currentTick = Current.Game.tickManager.TicksGame;
            var ticksRemaining = backingOrder.DeliveryTick - currentTick;
            Logging.WriteDebugMessage($"Ticking Order {OrderId}, State: {Status}, Ticks remaining: {ticksRemaining}");
            try
            {
                if (ticksRemaining >= TicksPerHalfDay || IsFinished) return;

                // State check
                switch (backingOrder.Status)
                {
                    case OrderStatusEnum.Delivered:
                    case OrderStatusEnum.Failed:
                    case OrderStatusEnum.Reversed:
                        break;
                    case OrderStatusEnum.Placed:
                        ProcessStatePlaced(currentTick, ticksRemaining);
                        break;
                    case OrderStatusEnum.OutForDelivery:
                        ProcessStateOutForDelivery(currentTick, ticksRemaining);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"An order failed to update: {e}");
            }
            finally
            {
                Busy = false;
                Logging.WriteDebugMessage($"Order {OrderId} tick finished");
            }
        }

        private void ProcessStatePlaced(int currentTick, int ticksRemaining)
        {
            if (ticksRemaining < TicksUntilDelivery)
            {
                SetOrderState(OrderStatusEnum.OutForDelivery, currentTick);
            }
        }

        private void ProcessStateOutForDelivery(int currentTick, int ticksRemaining)
        {
            // Don't do anything until we reach the delivery time.
            if (currentTick < backingOrder.DeliveryTick) return;

            SetOrderState(OrderStatusEnum.Delivered, currentTick);

            DeliverOrder();

            IsFinished = true;
        }

        private void DeliverOrder()
        {
            Logging.WriteDebugMessage($"Deliver Order {OrderId}");
            var colonyId = backingOrder.ColonyId;
            var orderItemCollection = IseCentral.DataCache.GetCollection<DBOrderItem>(Tables.OrderItems);
            var storedItemCollection = IseCentral.DataCache.GetCollection<DBStorageItem>(Tables.Delivered);
            storedItemCollection.EnsureIndex(item => item.ColonyId);
            storedItemCollection.EnsureIndex(item => item.ItemCode);

            var orderItems = orderItemCollection.Find(item => item.OrderId == OrderId);
            var deliveryItems = storedItemCollection.Find(item => item.ColonyId == colonyId).ToList();

            foreach (var orderItem in orderItems)
            {
                var item = deliveryItems.FirstOrDefault
                               (storageItem => storageItem.ItemCode == orderItem.ItemCode) ??
                           new DBStorageItem
                           {
                               StoredItemID = GetShaHash($"{colonyId}{orderItem.ItemCode}"),
                               ItemCode = orderItem.ItemCode,
                               ThingDef = orderItem.ThingDef,
                               Stuff = orderItem.Stuff,
                               Quality = orderItem.Quality,
                               Quantity = 0,
                               ColonyId = colonyId,
                               Value = 0
                           };
                item.Quantity += orderItem.Quantity;
                item.Value = (int) Math.Ceiling(GetValueForThing(item.ThingDef, item.Quality, item.Stuff))
                             * item.Quantity;
                storedItemCollection.Upsert(item);
            }

            // Remove delivered items.
            orderItemCollection.DeleteMany(item => item.OrderId == OrderId);
        }

        internal static void PopulateOrderItems(string orderId, OrderManifestReply orderManifest)
        {
            Logging.WriteDebugMessage($"Processing manifest for {orderId}");
            var db = IseCentral.DataCache;

            var orderItems = db.GetCollection<DBOrderItem>(Tables.OrderItems);
            orderItems.InsertBulk(
                orderManifest.Items.Select(
                    orderItem => new DBOrderItem
                    {
                        ItemCode = orderItem.ItemCode,
                        ThingDef = orderItem.ThingDef,
                        Stuff = orderItem.Stuff,
                        Quality = orderItem.Quality,
                        Quantity = orderItem.Quantity,
                        OrderId = orderId
                    }
                )
            );

            // Mark the order as downloaded so we don't fetch it again.
            var orderCollection = db.GetCollection<DBOrder>(Tables.Orders);
            var order = orderCollection.FindById(orderId);
            if (order == null)
            {
                throw new NullReferenceException($"Couldn't find order {orderId} in cache");
            }

            order.ManifestAvailable = true;
            orderCollection.Update(order);
        }

        private void SetOrderState(OrderStatusEnum state, int currentTick)
        {
            Logging.WriteDebugMessage($"Marking order {OrderId} as {state}");
            var colonyId = backingOrder.ColonyId;
            var db = IseCentral.DataCache;
            var orderCollection = db.GetCollection<DBOrder>(Tables.Orders);
            var gameComponent = Current.Game.GetComponent<ISEGameComponent>();
            var request = new OrderUpdateRequest()
            {
                ColonyId = colonyId,
                ClientBindId = gameComponent.ClientBind,
                ColonyTick = currentTick,
                OrderId = OrderId,
                Status = state
            };
            try
            {
                var result = SendAndParseReply(
                    request,
                    OrderStatusReply.Parser,
                    $"{URLPrefix}order/update",
                    Method.POST,
                    request.ClientBindId
                );
                if (result.Status != state)
                {
                    throw new InvalidOperationException($"Server refused status update to {state}");
                }

                // Update current order with values from server, just in case
                // an admin has changed them.
                backingOrder.PlacedTick = result.PlacedTick;
                backingOrder.DeliveryTick = result.DeliveryTick;
                backingOrder.Status = result.Status;
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to update order status for {OrderId}");
                Logging.WriteErrorMessage($"{e}");
                throw;
            }

            orderCollection.Update(backingOrder);
        }

        #endregion
    }
}