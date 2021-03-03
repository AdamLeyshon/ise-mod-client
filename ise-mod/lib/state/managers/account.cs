#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, account.cs, Created 2021-02-26

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ise.components;
using ise_core.db;
using Order;
using RestSharp;
using Steamworks;
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
        private bool busy;

        #endregion

        #region ctor

        internal Account(string colonyBindId, ISEGameComponent gc)
        {
            Logging.WriteDebugMessage($"Starting Account manager for {colonyBindId}");
            accountId = colonyBindId;
            gameComponent = gc;
            busy = false;
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
            var orderCollection = db.GetCollection<DBOrder>(Tables.Orders);
            var orders = orderCollection.Find(o => o.ColonyId == accountId);
            var itemCollection = db.GetCollection<DBOrderItem>(Tables.OrderItems);
            foreach (var dbOrder in orders) itemCollection.DeleteMany(item => item.OrderId == dbOrder.Id);
            orderCollection.DeleteMany(item => item.ColonyId == accountId);
            itemCollection.EnsureIndex(item => item.OrderId);
            itemCollection.EnsureIndex(item => item.ItemCode);
            orderCollection.EnsureIndex(order => order.Id);
        }

        internal void AddOrder(string orderId)
        {
            var db = IseCentral.DataCache;
            if (db.GetCollection<DBOrder>(Tables.Orders).FindById(orderId) == null)
                throw new KeyNotFoundException($"Unable to load order {orderId} from Database");
            if (!activeOrders.ContainsKey(orderId))
            {
                activeOrders.Add(orderId, new Order(orderId));
            }
        }

        /// <summary>
        ///     Update order list and update DB order entries
        ///     This func should be called in an async task to
        ///     stop it from blocking the game thread.
        /// </summary>
        internal async void UpdateAsync()
        {
            if (busy) return;
            var currentTick = Current.Game.tickManager.TicksGame;
            try
            {
                busy = true;
                var ordersToProcess = new List<string>();
                var db = IseCentral.DataCache;
                var dbOrders = db.GetCollection<DBOrder>(Tables.Orders);
                var tasks = new List<Task>();

                // Get order list if we've passed the update date.
                if (currentTick >= nextUpdate)
                {
                    nextUpdate = currentTick + OrderUpdateTickRate;
                    
                    // Download list of orders from server
                    Logging.WriteDebugMessage($"Synchronising orders with server");
                    var orders = GetOrderList().Orders;
                    Logging.WriteDebugMessage($"Got {orders.Count} orders to process from server");
                    
                    // Find orders in a valid state.
                    foreach (var statusReply in orders
                        .Where(sr => sr.Status == OrderStatusEnum.Placed || sr.Status == OrderStatusEnum.OutForDelivery
                        )
                    )
                    {
                        // If no existing order, create one, we'll download the items after.
                        var dbOrder = dbOrders.FindById(statusReply.OrderId) ?? new DBOrder
                        {
                            Id = statusReply.OrderId,
                            ManifestAvailable = false,
                            PlacedTick = statusReply.PlacedTick,
                            ColonyId = accountId,
                        };
                        dbOrder.Status = statusReply.Status;
                        dbOrder.DeliveryTick = statusReply.DeliveryTick;
                        Logging.WriteDebugMessage($"Added/Updated Order {dbOrder.Id}");
                        dbOrders.Upsert(dbOrder);
                    }

                    // Need to get items for these orders.
                    ordersToProcess.AddRange(dbOrders.Find(order => !order.ManifestAvailable)
                        .Select(order => order.Id));
                    Logging.WriteDebugMessage($"Need to get {ordersToProcess.Count} Manifests");

                    tasks.AddRange(ordersToProcess.Select(order =>
                        Task.Run(() => Order.PopulateOrderItems(order, GetOrderManifest(order)))));
                    foreach (var task in tasks)
                    {
                        await task;
                        if (task.IsFaulted)
                        {
                            throw new InvalidOperationException($"Failed to get or process manifest: {task.Exception}");
                        }
                    }
                }

                tasks.Clear();
                ordersToProcess.Clear();

                // Spawn order trackers for any orders that don't have one but have the manifest downloaded.
                foreach (var orderId in dbOrders.Find(order => order.ManifestAvailable).Select(order => order.Id))
                {
                    if (!activeOrders.ContainsKey(orderId))
                    {
                        activeOrders.Add(orderId, new Order(orderId));
                    }
                }

                foreach (var order in activeOrders.Values.Where(order => !order.Busy))
                {
                    if (order.IsFinished)
                    {
                        // If the order is done, don't tick it, just remove it
                        // from the active order list after iteration.
                        ordersToProcess.Add(order.OrderId);
                        continue;
                    }

                    tasks.Add(Task.Run(() => order.Update()));
                }

                foreach (var task in tasks)
                {
                    await task;
                    if (!task.IsFaulted) continue;
                    throw new InvalidOperationException($"An order failed to update: {task.Exception}");
                }

                // Remove dead orders collected above.
                foreach (var orderId in ordersToProcess)
                {
                    activeOrders.Remove(orderId);
                }
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to process orders for account {accountId}");
                Logging.WriteErrorMessage($"{e}");
            }
            finally
            {
                Logging.WriteDebugMessage($"Finished Account Manager Update for {accountId}");
                busy = false; // Set not busy in case we have a chance to recover.
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
            try
            {
                return SendAndParseReply(
                    request,
                    OrderListReply.Parser,
                    $"{URLPrefix}order/list",
                    Method.POST,
                    request.ClientBindId
                );
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to download order list for {accountId}");
                Logging.WriteErrorMessage($"{e}");
                throw;
            }
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

            try
            {
                return SendAndParseReply(
                    request,
                    OrderManifestReply.Parser,
                    $"{URLPrefix}order/manifest",
                    Method.POST,
                    request.ClientBindId
                );
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to download manifest for {orderId}");
                Logging.WriteErrorMessage($"{e}");
                throw;
            }
        }

        #endregion
    }
}