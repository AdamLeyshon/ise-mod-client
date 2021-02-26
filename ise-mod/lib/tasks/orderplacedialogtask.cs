#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, orderplacedialogtask.cs, Created 2021-02-18

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bank;
using ise.components;
using ise.dialogs;
using ise_core.db;
using LiteDB;
using Order;
using RestSharp;
using RimWorld;
using Verse;
using static ise.lib.Constants;
using static ise.lib.Tradables;
using static ise_core.rest.Helpers;
using static ise_core.rest.api.v1.Constants;
using static ise.lib.Cache;

namespace ise.lib.tasks
{
    internal class OrderPlaceDialogTask : AbstractDialogTask
    {
        #region Fields

        private readonly int additionalFunds;
        private readonly int gameTick;
        private readonly ISEGameComponent gc;
        private readonly Pawn pawn;

        private State state;
        private Task task;

        #endregion

        #region ctor

        public OrderPlaceDialogTask(IDialog dialog, Pawn userPawn, int additionalFunds) : base(dialog)
        {
            state = State.Start;
            gc = Current.Game.GetComponent<ISEGameComponent>();
            pawn = userPawn;
            this.additionalFunds = additionalFunds;
            gameTick = Current.Game.tickManager.TicksGame;
        }

        #endregion

        #region Properties

        public OrderReply Reply { get; private set; }
        public OrderRequest Request { get; private set; }

        #endregion

        #region Methods

        public override void Update()
        {
            // Handle task errors first.
            if (task != null && task.IsFaulted) LogTaskError();

            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartOrder();
                    break;
                case State.Place:
                    Dialog.DialogMessage = "Placing order";
                    if (task != null && task.IsCompleted) ProcessOrderRequestReply(((Task<OrderReply>) task).Result);

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
            state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {task.Exception}");
            task = null;
        }

        private void StartOrder()
        {
            var colonyId = gc.GetColonyId(pawn.Map);

            task = new Task<OrderReply>(delegate
            {
                using (var db = new LiteDatabase(DBLocation))
                {
                    var promise = db.GetCollection<DBInventoryPromise>(PromiseTable).FindById(gc.GetColonyId(pawn.Map));

                    if (promise.InventoryPromiseExpires < GetUTCNow())
                    {
                        state = State.Error;
                        return null;
                    }

                    var request = new OrderRequest
                    {
                        ColonyId = colonyId,
                        ClientBindId = gc.ClientBind,
                        Currency = BankCurrency.Silver,
                        InventoryPromiseId = promise.InventoryPromiseId,
                        AdditionalFunds = additionalFunds,
                        ColonyTick = gameTick,
                    };

                    var colonyBasket = GetCache(db, colonyId, CacheType.ColonyBasket).FindAll();
                    var marketBasket = GetCache(db, colonyId, CacheType.MarketBasket).FindAll();

                    request.WantToSell.AddRange(CachedTradablesToOrderItems(colonyBasket));
                    request.WantToBuy.AddRange(CachedTradablesToOrderItems(marketBasket));

                    Request = request.Clone();

                    return SendAndParseReply(
                        request,
                        OrderReply.Parser,
                        $"{URLPrefix}order/place",
                        Method.POST,
                        gc.ClientBind
                    );
                }
            });
            task.Start();
            state = State.Place;
        }

        private void ProcessOrderRequestReply(OrderReply reply)
        {
            if (reply == null || reply.Status == OrderRequestStatus.Rejected)
            {
                state = State.Error;
                return;
            }

            Reply = reply.Clone();
            var db = new LiteDatabase(DBLocation);
            try
            {
                db.GetCollection<DBOrder>(OrderTable).Insert(
                    new DBOrder
                    {
                        Id = reply.Data.OrderId,
                        ColonyId = gc.GetColonyId(pawn.Map),
                        Status = (int) reply.Data.Status,
                        PlacedTick = gameTick,
                        DeliveryTick = reply.Data.DeliveryTick
                    });

                RemoveTradedGoods(reply);

                // Clear all cache and promises
                var colonyId = gc.GetColonyId(pawn.Map);
                db.GetCollection<DBInventoryPromise>().DeleteMany(x => x.ColonyId == colonyId);
                DropCache(db, colonyId, CacheType.ColonyBasket);
                DropCache(db, colonyId, CacheType.MarketBasket);
                DropCache(db, colonyId, CacheType.ColonyCache);
                DropCache(db, colonyId, CacheType.MarketCache);
            }
            catch (Exception e)
            {
                state = State.Error;
                return;
            }

            state = State.Done;
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
            var colonyId = gc.GetColonyId(pawn.Map);
            using (var db = new LiteDatabase(DBLocation))
            {
                var colonyBasket = GetCache(db, colonyId, CacheType.ColonyBasket).FindAll();

                // Start with the smallest pile of silver and destroy until we've removed as much as we need to
                if (Request.AdditionalFunds > 0)
                    RemoveGoods(
                        ThingDefSilver,
                        100,
                        Request.AdditionalFunds,
                        pawn.Map
                    );

                foreach (var soldItem in colonyBasket)
                    RemoveGoods(soldItem.ThingDef,
                        soldItem.HitPoints,
                        soldItem.TradedQuantity,
                        pawn.Map,
                        soldItem.Quality,
                        soldItem.Stuff
                    );
            }
        }

        /// <summary>
        /// Delete the traded goods from the colony since the order has succeeded.
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
            Logging.WriteDebugMessage($"Looking for {thingDef} x {quantity} to remove");

            if (quantity == 0)
            {
                Logging.WriteDebugMessage($"Asked to remove {thingDef} x 0, This is an error");
                return;
            }

            var things = GetItemsNearBeacons(map, thingDef).ToList();
            Logging.WriteDebugMessage($"I found {thingDef} x {things.Count()}");
            if (quality >= 2)
            {
                Logging.WriteDebugMessage($"Filtering for quality: {quality}");
                things = things.Where(x =>
                {
                    var thing = x.GetInnerIfMinified();
                    var possibleQuality = thing.TryGetComp<CompQuality>();
                    return possibleQuality != null && (int) possibleQuality.Quality == quality;
                }).ToList();
                Logging.WriteDebugMessage($"I found {thingDef} x {things.Count()}");
            }

            if (stuff != null)
            {
                Logging.WriteDebugMessage($"Filtering for stuff: {stuff}");

                things = things.Where(x =>
                {
                    var thing = x.GetInnerIfMinified();
                    return thing.Stuff.defName == stuff;
                }).ToList();
                Logging.WriteDebugMessage($"I found {thingDef} x {things.Count()}");
            }

            var thingsToRemove = things.ToList();
            thingsToRemove.SortBy(x => x.stackCount);

            Logging.WriteDebugMessage($"Found {thingsToRemove.Count()} stacks to inspect");

            var toRemove = quantity;
            foreach (var stack in thingsToRemove)
            {
                if (CalculateThingHitPoints(stack) != hitPoints)
                    continue;


                Logging.WriteDebugMessage($"Trying to remove: {stack} x {quantity}");

                var newStack = stack.stackCount > toRemove
                    ? stack.SplitOff(toRemove)
                    : stack;

                toRemove -= newStack.stackCount;

                if (newStack is MinifiedThing && newStack.stackCount == 1)
                {
                    Logging.WriteDebugMessage($"Destroying Minified thing {stack}");
                    stack.Destroy();
                }
                else
                {
                    newStack.Destroy();
                }

                if (toRemove == 0) break;
            }

            if (toRemove > 0)
                Logging.WriteErrorMessage($"We've been had, the colony doesn't have enough {thingDef} to remove.");
        }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Place,
            Done,
            Error,
        }

        #endregion
    }
}