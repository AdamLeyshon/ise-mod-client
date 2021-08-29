#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, account.cs 2021-02-26

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colony;
using ise.components;
using ise_core.db;
using Order;
using RestSharp;
using RimWorld;
using Verse;
using static ise.lib.Constants;
using static ise_core.rest.Helpers;
using static ise_core.rest.api.v1.Constants;

namespace ise.lib.state.managers
{
    internal class Account
    {
        #region ctor

        internal Account(string colonyBindId, ISEGameComponent gc)
        {
            Logging.WriteDebugMessage($"Starting Account manager for {colonyBindId}");
            _accountId = colonyBindId;
            _gameComponent = gc;
            _busy = false;
            _activeOrders = new Dictionary<string, Order>();
            _nextUpdate = 0; // UpdateAsync on next rare tick call.

            // Delete old orders from DB, new list will be pulled on next update.
            FlushOrders(Current.Game.tickManager.TicksGame);
        }

        #endregion

        #region Fields

        private readonly string _accountId;
        private readonly Dictionary<string, Order> _activeOrders;
        private readonly ISEGameComponent _gameComponent;
        private int _nextUpdate;
        private bool _busy;

        #endregion

        #region Methods

        private void FlushOrders(int currentTick)
        {
            var db = IseCentral.DataCache;
            var orderCollection = db.GetCollection<DBOrder>(Tables.Orders);
            var orders = orderCollection.Find(o => o.ColonyId == _accountId);
            var itemCollection = db.GetCollection<DBOrderItem>(Tables.OrderItems);

            itemCollection.EnsureIndex(item => item.OrderId);
            itemCollection.EnsureIndex(item => item.ItemCode);
            orderCollection.EnsureIndex(order => order.Id);

            var ordersToProcess = new List<string>();
            Logging.WriteDebugMessage("Deleting delivered orders from the future");

            if (orders == null) return;

            var dbOrders = orders.ToList();
            ordersToProcess.AddRange(
                dbOrders.Where(order => order.DeliveryTick > currentTick).Select(order => order.Id));

            var dbDeliveredItems = IseCentral.DataCache.GetCollection<DBStorageItem>(Tables.Delivered);

            foreach (var orderId in ordersToProcess)
            {
                // Get the order manifest
                var orderItems = itemCollection.Find(m => m.OrderId == orderId);
                foreach (var orderItem in orderItems)
                {
                    // Find the items that were delivered
                    var storedItem = dbDeliveredItems.FindOne(item =>
                        item.ColonyId == _accountId && item.ItemCode == orderItem.ItemCode);

                    if (storedItem == null) continue;

                    // Reverse the transaction
                    storedItem.Quantity -= orderItem.Quantity;
                    if (storedItem.Quantity <= 0)
                        // Delete the item if there's none left
                        dbDeliveredItems.Delete(storedItem.StoredItemID);
                }
            }

            foreach (var dbOrder in dbOrders) itemCollection.DeleteMany(item => item.OrderId == dbOrder.Id);
            orderCollection.DeleteMany(item => item.ColonyId == _accountId);
        }

        internal void AddOrder(string orderId)
        {
            var db = IseCentral.DataCache;
            if (db.GetCollection<DBOrder>(Tables.Orders).FindById(orderId) == null)
                throw new KeyNotFoundException($"Unable to load order {orderId} from Database");
            if (!_activeOrders.ContainsKey(orderId)) _activeOrders.Add(orderId, new Order(orderId));
        }

        /// <summary>
        ///     Update order list and update DB order entries
        ///     This func should be called in an async task to
        ///     stop it from blocking the game thread.
        /// </summary>
        internal async void UpdateAsync()
        {
            if (_busy) return;
            var currentTick = Current.Game.tickManager.TicksGame;
            try
            {
                _busy = true;
                var ordersToProcess = new List<string>();
                var db = IseCentral.DataCache;
                var dbOrders = db.GetCollection<DBOrder>(Tables.Orders);
                var tasks = new List<Task>();

                // Get order list if we've passed the update date.
                if (currentTick >= _nextUpdate)
                {
                    _nextUpdate = currentTick + OrderUpdateTickRate;

                    // Update the server with our current tick, so that any orders can be rolled back as required.
                    Logging.WriteDebugMessage("Synchronising colony data with server");
                    UpdateColonyStatus(currentTick);

                    // Download list of orders from server
                    Logging.WriteDebugMessage("Synchronising orders with server");
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
                            ColonyId = _accountId
                        };
                        dbOrder.Status = statusReply.Status;
                        dbOrder.DeliveryTick = statusReply.DeliveryTick;
                        Logging.WriteDebugMessage($"Added/Updated Order {dbOrder.Id}");
                        dbOrders.Upsert(dbOrder);
                    }

                    // Need to get items for these orders.
                    ordersToProcess.AddRange(dbOrders
                        .Find(order => !order.ManifestAvailable && order.ColonyId == _accountId)
                        .Select(order => order.Id));
                    Logging.WriteDebugMessage($"Need to get {ordersToProcess.Count} Manifests");

                    tasks.AddRange(ordersToProcess.Select(order =>
                        Task.Run(() => Order.PopulateOrderItems(order, GetOrderManifest(order)))));
                    foreach (var task in tasks)
                    {
                        await task;
                        if (task.IsFaulted)
                            throw new InvalidOperationException($"Failed to get or process manifest: {task.Exception}");
                    }
                }

                tasks.Clear();
                ordersToProcess.Clear();

                // Spawn order trackers for any orders that don't have one but have the manifest downloaded.
                // Also refresh the backing data, since the server may have changed it.
                foreach (var orderId in dbOrders.Find(order => order.ManifestAvailable).Select(order => order.Id))
                    if (!_activeOrders.ContainsKey(orderId))
                        _activeOrders.Add(orderId, new Order(orderId));
                    else
                        _activeOrders[orderId].RefreshBackingData();

                foreach (var order in _activeOrders.Values.Where(order => !order.Busy))
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
                foreach (var orderId in ordersToProcess) _activeOrders.Remove(orderId);
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to process orders for account {_accountId}");
                Logging.WriteErrorMessage($"{e}");
            }
            finally
            {
                Logging.WriteDebugMessage($"Finished Account Manager Update for {_accountId}");
                _busy = false; // Set not busy in case we have a chance to recover.
            }
        }

        private void UpdateColonyStatus(int currentTick)
        {
            var request = new ColonyUpdateRequest
            {
                ClientBindId = _gameComponent.ClientBind,
                Data = new ColonyData
                {
                    ColonyId = _accountId,
                    Tick = currentTick,
                    UsedDevMode = _gameComponent.SpawnToolUsed,
                    GameVersion = VersionControl.CurrentVersionStringWithRev
                }
            };

            SendAndParseReply(
                request,
                ColonyData.Parser,
                $"{URLPrefix}colony/",
                Method.PATCH,
                _gameComponent.ClientBind
            );
        }

        private OrderListReply GetOrderList()
        {
            Logging.WriteDebugMessage($"Fetching order list for {_accountId}");
            try
            {
                return ise_core.rest.api.v1.Order.GetOrderList(_gameComponent.ClientBind, _accountId);
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to download order list for {_accountId}");
                Logging.WriteErrorMessage($"{e}");
                throw;
            }
        }

        private OrderManifestReply GetOrderManifest(string orderId)
        {
            Logging.WriteDebugMessage($"Fetching manifest for {orderId}");
            try
            {
                return ise_core.rest.api.v1.Order.GetOrderManifest(_gameComponent.ClientBind, _accountId, orderId);
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"Failed to download manifest for {orderId}");
                Logging.WriteErrorMessage($"{e}");
                throw;
            }
        }

        public IEnumerable<Order> GetActiveOrders => _activeOrders.Values;

        #endregion
    }
}