#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, account.cs, Created 2021-02-26

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ise.components;
using ise_core.db;
using Order;
using RestSharp;
using Verse;
using static ise.lib.Constants;
using static ise_core.rest.Helpers;
using static ise_core.rest.api.v1.Constants;

namespace ise.lib.state.managers
{
    internal class Account
    {
        #region Fields

        private readonly string accountId;
        private readonly Dictionary<string, Order> activeOrders;
        private readonly ISEGameComponent gameComponent;
        private int nextUpdate;

        #endregion

        #region ctor

        internal Account(string colonyBindId, ISEGameComponent gc)
        {
            Logging.WriteDebugMessage($"Starting Account manager for {colonyBindId}");
            accountId = colonyBindId;
            gameComponent = gc;
            activeOrders = new Dictionary<string, Order>();
            nextUpdate = 0; // UpdateAsync on next rare tick call.

            // Delete old orders from DB, new list will be pulled on next update.
            FlushOrders();
        }

        #endregion

        #region Methods

        private void FlushOrders()
        {
            var db = IseCentral.DataCache;
            var orders = db.GetCollection<DBOrder>(Tables.Orders).Find(o => o.ColonyId == accountId);
            var orderItems = db.GetCollection<DBOrderItem>(Tables.OrderItems);
            foreach (var dbOrder in orders) orderItems.DeleteMany(item => item.OrderId == dbOrder.Id);
        }

        internal void AddOrder(string orderId)
        {
            var db = IseCentral.DataCache;
            if (db.GetCollection<DBOrder>(Tables.Orders).FindById(orderId) == null)
                throw new KeyNotFoundException($"Unable to load order {orderId} from Database");

            activeOrders.Add(orderId, new Order(orderId));
        }

        /// <summary>
        ///     Update order list and update DB order entries
        ///     This func should be called in an async task to
        ///     stop it from blocking the game thread.
        /// </summary>
        internal async void UpdateAsync()
        {
            var currentTick = Current.Game.tickManager.TicksGame;
            var orderManifestsToFetch = new List<string>();
            if (currentTick < nextUpdate) return;
            nextUpdate = currentTick + OrderUpdateTickRate;

            // Get order list
            var orders = GetOrderList().Orders;
            var db = IseCentral.DataCache;
            var dbOrders = db.GetCollection<DBOrder>(Tables.Orders);
            foreach (var statusReply in orders
                .Where(sr => sr.Status == OrderStatusEnum.Placed || sr.Status == OrderStatusEnum.OutForDelivery
                )
            )
            {
                // If no existing order, create one, we'll download the items after.
                var dbOrder = dbOrders.FindById(statusReply.OrderId);
                if (dbOrder == null)
                {
                    dbOrder = new DBOrder
                    {
                        Id = statusReply.OrderId
                    };
                    // Need to get items for this order.
                    orderManifestsToFetch.Add(statusReply.OrderId);
                }

                dbOrder.Status = statusReply.Status;
                dbOrder.DeliveryTick = statusReply.DeliveryTick;
                dbOrders.Upsert(dbOrder);
            }

            var tasks = new List<Task>();
            foreach (var t in orderManifestsToFetch)
            {
                var task = new Task(delegate { Order.PopulateOrderItems(t, GetOrderManifest(t)); }
                );
                task.Start();
                tasks.Add(task);
                activeOrders.Add(t, new Order(t));
            }

            foreach (var task in tasks) await task;

            var toRemove = new List<string>();
            foreach (var order in activeOrders.Values.Where(order => !order.Busy))
            {
                if (order.CanRemove)
                {
                    toRemove.Add(order.OrderId);
                    continue;
                }

                var task = new Task(delegate { order.Update(); });
                task.Start();
            }

            // Remove dead orders
            foreach (var orderId in toRemove)
            {
                activeOrders.Remove(orderId);
            }
        }

        private OrderListReply GetOrderList()
        {
            Logging.WriteDebugMessage($"Fetching order list for {accountId}");

            var request = new OrderListRequest
            {
                ColonyId = accountId,
                ClientBindId = gameComponent.ClientBind,
                Any = false
            };

            return SendAndParseReply(
                request,
                OrderListReply.Parser,
                $"{URLPrefix}order/list",
                Method.POST,
                request.ClientBindId
            );
        }

        private OrderManifestReply GetOrderManifest(string orderId)
        {
            Logging.WriteDebugMessage($"Fetching manifest for {orderId}");
            var request = new OrderManifestRequest
            {
                ColonyId = accountId,
                ClientBindId = gameComponent.ClientBind,
                OrderId = orderId
            };

            return SendAndParseReply(
                request,
                OrderManifestReply.Parser,
                $"{URLPrefix}order/manifest",
                Method.POST,
                request.ClientBindId
            );
        }



        #endregion
    }
}