#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, orderplacedialogtask.cs 2021-02-18

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using ise.components;
using ise.dialogs;
using ise_core.db;
using Order;
using RimWorld;
using Verse;
using static ise.lib.Constants;
using static ise.lib.Tradables;
using static ise_core.rest.Helpers;
using static ise.lib.Cache;

namespace ise.lib.tasks
{
    internal class OrderPlaceDialogTask : AbstractDialogTask
    {
        #region ctor

        public OrderPlaceDialogTask(IDialog dialog, Pawn userPawn, int additionalFunds) : base(dialog)
        {
            _state = State.Start;
            _gc = Current.Game.GetComponent<ISEGameComponent>();
            _pawn = userPawn;
            _additionalFunds = additionalFunds;
            _gameTick = Current.Game.tickManager.TicksGame;
        }

        #endregion

        #region Properties

        public OrderReply Reply { get; private set; }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Place,
            Done,
            Error
        }

        #endregion

        #region Fields

        private readonly int _additionalFunds;
        private readonly int _gameTick;
        private readonly ISEGameComponent _gc;
        private readonly Pawn _pawn;

        private State _state;
        private Task _task;

        #endregion

        #region Methods

        public override void Update()
        {
            // Handle task errors first.
            if (_task != null && _task.IsFaulted) LogTaskError();

            switch (_state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartOrder();
                    break;
                case State.Place:
                    Dialog.DialogMessage = "Placing order";
                    if (_task != null && _task.IsCompleted) ProcessOrderRequestReply(((Task<OrderReply>)_task).Result);
                    break;
                case State.Done:
                    Dialog.DialogMessage = "Order was placed";
                    Done = true;
                    break;
                case State.Error:
                    Dialog.DialogMessage = "Order was not placed";
                    Done = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogTaskError()
        {
            _state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {_task.Exception}");
            _task = null;
        }

        private void StartOrder()
        {
            var colonyId = _gc.GetColonyId(_pawn.Map);

            _task = new Task<OrderReply>(delegate
            {
                var db = IseCentral.DataCache;
                var promise = db.GetCollection<DBInventoryPromise>(Tables.Promises)
                    .FindById(_gc.GetColonyId(_pawn.Map));

                if (promise.InventoryPromiseExpires < GetUTCNow())
                {
                    _state = State.Error;
                    return null;
                }

                var colonyBasket = GetCache(colonyId, CacheType.ColonyBasket).FindAll();
                var marketBasket = GetCache(colonyId, CacheType.MarketBasket).FindAll();

                return ise_core.rest.api.v1.Order.PlaceOrder(
                    _gc.ClientBind,
                    colonyId,
                    promise.InventoryPromiseId,
                    _gameTick,
                    CachedTradablesToOrderItems(marketBasket),
                    CachedTradablesToOrderItems(colonyBasket),
                    CurrencyEnum.Utc,
                    _additionalFunds
                );
            });
            _task.Start();
            _state = State.Place;
        }

        private void ProcessOrderRequestReply(OrderReply reply)
        {
            if (reply == null || reply.Status == OrderRequestStatus.Rejected)
            {
                _state = State.Error;
                return;
            }

            Reply = reply.Clone();
            try
            {
                IseCentral.DataCache.BeginTrans();
                IseCentral.DataCache.GetCollection<DBOrder>(Tables.Orders).Insert(
                    new DBOrder
                    {
                        Id = reply.Data.OrderId,
                        ColonyId = _gc.GetColonyId(_pawn.Map),
                        Status = reply.Data.Status,
                        PlacedTick = _gameTick,
                        DeliveryTick = reply.Data.DeliveryTick
                    });

                RemoveTradedGoods(reply);

                // Clear all cache and promises
                var colonyId = _gc.GetColonyId(_pawn.Map);
                IseCentral.DataCache.GetCollection<DBInventoryPromise>().DeleteMany(x => x.ColonyId == colonyId);
                DropCache(colonyId, CacheType.ColonyBasket);
                DropCache(colonyId, CacheType.MarketBasket);
                DropCache(colonyId, CacheType.ColonyCache);
                DropCache(colonyId, CacheType.MarketCache);
                
                IseCentral.DataCache.Commit();
                
                // Start tracking the Order Status
                _gc.GetAccount(colonyId).AddOrder(reply.Data.OrderId);
            }
            catch (Exception)
            {
                IseCentral.DataCache.Rollback();
                _state = State.Error;
                return;
            }

            _state = State.Done;
        }

        private static IEnumerable<OrderItem> CachedTradablesToOrderItems(IEnumerable<DBCachedTradable> cts)
        {
            return cts.Select(cachedTradable => new OrderItem
            {
                ItemCode = cachedTradable.ItemCode,
                Quantity = cachedTradable.TradedQuantity,
                Health = cachedTradable.HitPoints
            });
        }

        private void RemoveTradedGoods(OrderReply reply)
        {
            var colonyId = _gc.GetColonyId(_pawn.Map);
            var colonyBasket = GetCache(colonyId, CacheType.ColonyBasket).FindAll();

            // Start with the smallest pile of silver and destroy until we've removed as much as we need to
            if (_additionalFunds > 0)
                RemoveGoods(
                    ThingDefSilver,
                    100,
                    _additionalFunds,
                    _pawn.Map
                );

            foreach (var soldItem in colonyBasket)
                RemoveGoods(soldItem.ThingDef,
                    soldItem.HitPoints,
                    soldItem.TradedQuantity,
                    _pawn.Map,
                    soldItem.Quality,
                    soldItem.Stuff
                );
        }

        /// <summary>
        ///     Delete the traded goods from the colony since the order has succeeded.
        /// </summary>
        /// <param name="thingDef">Item ThingDef</param>
        /// <param name="hitPoints">The HP as integer percentage</param>
        /// <param name="quantity">Quantity to remove</param>
        /// <param name="map">The map to delete items from</param>
        /// <param name="quality">Thing numeric quality</param>
        /// <param name="stuff">ThingDef of Stuff</param>
        private static void RemoveGoods(
            string thingDef,
            int hitPoints,
            int quantity,
            Map map,
            int quality = 0,
            string stuff = null)
        {
            // Sort the Things, start with smallest stacks first.
            Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Looking for {thingDef} x {quantity} to remove");

            if (quantity == 0)
            {
                Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Asked to remove {thingDef} x 0, This is an error");
                return;
            }

            var things = GetItemsNearBeacons(map, thingDef).ToList();
            Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"I found {thingDef} x {things.Count}");
            if (quality >= 2)
            {
                Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Filtering for quality: {quality}");
                things = things.Where(x =>
                {
                    var thing = x.GetInnerIfMinified();
                    var possibleQuality = thing.TryGetComp<CompQuality>();
                    return possibleQuality != null && (int)possibleQuality.Quality == quality;
                }).ToList();
                Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"I found {thingDef} x {things.Count}");
            }

            if (stuff != null)
            {
                Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Filtering for stuff: {stuff}");

                things = things.Where(x =>
                {
                    var thing = x.GetInnerIfMinified();
                    return thing.Stuff.defName == stuff;
                }).ToList();
                Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"I found {thingDef} x {things.Count}");
            }

            var thingsToRemove = things.ToList();
            thingsToRemove.SortBy(x => x.stackCount);

            Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Found {thingsToRemove.Count} stacks to inspect");

            var toRemove = quantity;
            foreach (var stack in thingsToRemove)
            {
                if (CalculateThingHitPoints(stack) != hitPoints)
                    continue;

                Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Trying to remove: {stack} x {quantity}");

                var newStack = stack.stackCount > toRemove
                    ? stack.SplitOff(toRemove)
                    : stack;

                toRemove -= newStack.stackCount;

                if (newStack is MinifiedThing && newStack.stackCount == 1)
                {
                    Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons ,$"Destroying Minified thing {stack}");
                    stack.Destroy();
                }
                else
                {
                    newStack.Destroy();
                }

                if (toRemove == 0) break;
            }

            if (toRemove > 0)
                Logging.WriteErrorMessage($"The colony doesn't have enough {thingDef} {stuff} {quality} to remove.");
        }

        #endregion
    }
}