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
using Verse;
using static ise.lib.Constants;
using static RimWorld.GenDate;
using static ise.lib.Tradables;

namespace ise.lib.state.managers
{
    internal class Order
    {
        #region Fields

        private const int TicksPerHalfDay = TicksPerDay / 2;

        internal string OrderId { get; private set; }
        private readonly DBOrder backingOrder;

        #endregion

        #region ctor

        internal Order(string orderId)
        {
            OrderId = orderId;
            var db = IseCentral.DataCache;
            backingOrder = db.GetCollection<DBOrder>(Tables.Orders).FindById(this.OrderId);
            Status = backingOrder.Status;
        }

        #endregion

        #region Properties

        internal OrderStatusEnum Status { get; private set; }
        internal bool Busy { get; private set; }
        internal bool CanRemove { get; private set; }

        #endregion

        #region Methods

        internal void Update()
        {
            Busy = true;
            Status = backingOrder.Status;
            var currentTick = Current.Game.tickManager.TicksGame;
            var ticksRemaining = backingOrder.DeliveryTick - currentTick;
            Logging.WriteDebugMessage($"Ticking Order {OrderId}, State: {Status}, Ticks remaining: {ticksRemaining}");
            if (ticksRemaining < TicksPerHalfDay)
                // State check
                switch (backingOrder.Status)
                {
                    case OrderStatusEnum.Delivered:
                    case OrderStatusEnum.Failed:
                    case OrderStatusEnum.Reversed:
                        break;
                    case OrderStatusEnum.Placed:
                        ProcessStatePlaced(currentTick);
                        break;
                    case OrderStatusEnum.OutForDelivery:
                        ProcessStateOutForDelivery(currentTick);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            Busy = false;
        }

        private void ProcessStatePlaced(int currentTick)
        {
        }

        private void ProcessStateOutForDelivery(int currentTick)
        {
            // Don't do anything until we reach the delivery time.
            if (currentTick < backingOrder.DeliveryTick) return;

            // Do something
            DeliverOrder();
        }

        private void DeliverOrder()
        {
            Logging.WriteDebugMessage($"Deliver Order {OrderId}");
            var colonyId = backingOrder.ColonyId;
            var orderItemCollection = IseCentral.DataCache.GetCollection<DBOrderItem>();
            var storedItemCollection = IseCentral.DataCache.GetCollection<DBStorageItem>();
            storedItemCollection.EnsureIndex(item => item.ColonyId);
            storedItemCollection.EnsureIndex(item => item.ItemCode);

            var orderItems = orderItemCollection.Find(item => item.OrderId == OrderId);
            var deliveryItems = IseCentral.DataCache.GetCollection<DBStorageItem>()
                .Find(item => item.ColonyId == colonyId).ToList();


            foreach (var orderItem in orderItems)
            {
                var item = deliveryItems.FirstOrDefault
                               (storageItem => storageItem.ItemCode == orderItem.ItemCode) ??
                           new DBStorageItem
                           {
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
            orderItems.EnsureIndex(item => item.OrderId);
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
        }

        #endregion
    }
}