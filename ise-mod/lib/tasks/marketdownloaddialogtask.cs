#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, marketdownloaddialogtask.cs, Created 2021-02-12

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory;
using ise.components;
using ise.dialogs;
using ise_core.db;
using LiteDB;
using RestSharp;
using RimWorld;
using UnityEngine;
using Verse;
using static ise_core.rest.Helpers;
using static ise.lib.Constants;
using static ise_core.rest.api.v1.Constants;
using static ise.lib.Tradables;
using static ise.lib.Cache;

namespace ise.lib.tasks
{
    internal class MarketDownloadDialogTask : AbstractDialogTask
    {
        #region Fields

        private readonly string colonyId;
        private readonly ISEGameComponent gc;
        private readonly Pawn pawn;
        private List<Thing> colonyThings;

        private State state;
        private Task task;

        #endregion

        #region ctor

        public MarketDownloadDialogTask(IDialog dialog, Pawn userPawn) : base(dialog)
        {
            pawn = userPawn;
            state = State.Start;
            gc = Current.Game.GetComponent<ISEGameComponent>();
            if (pawn == null) throw new ArgumentNullException(nameof(userPawn));

            colonyId = gc.GetColonyId(pawn.Map);
        }

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
                    StartDownload();
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Downloading Market Data";
                    if (task != null && task.IsCompleted) ProcessInventoryReply(((Task<InventoryReply>) task).Result);

                    break;
                case State.MarketCaching:
                    Dialog.DialogMessage = "Building Cache";
                    if (task != null && task.IsCompleted) GatherColonyInventory();

                    break;
                case State.ColonyCaching:
                    Dialog.DialogMessage = "Getting colony inventory";
                    if (task != null && task.IsCompleted) state = State.Done;

                    break;

                case State.Done:
                    Dialog.DialogMessage = "Market OK";
                    Done = true;
                    break;
                case State.Error:
                    Dialog.CloseDialog();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogTaskError()
        {
            state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {task.Exception}");
            if (task.Exception?.InnerExceptions != null)
                foreach (var innerException in task.Exception.InnerExceptions)
                    Logging.WriteErrorMessage($"Inner exception in task {innerException}");

            task = null;
        }

        private void StartDownload()
        {
            DBInventoryPromise inventoryPromise;
            // Open database (or create if doesn't exist)
            var db = IseCentral.DataCache;
            inventoryPromise = db.GetCollection<DBInventoryPromise>(Tables.Promises).FindById(colonyId);


            if (inventoryPromise != null && inventoryPromise.InventoryPromiseExpires > GetUTCNow())
            {
                // No need to download, promise is still valid
                GatherColonyInventory();
            }
            else
            {
                var request = new InventoryRequest {ColonyId = colonyId, ClientBindId = gc.ClientBind};
                Logging.WriteMessage("Asking server for Inventory");
                task = SendAndParseReplyAsync(
                    request,
                    InventoryReply.Parser,
                    $"{URLPrefix}inventory/",
                    Method.POST,
                    gc.ClientBind
                );
                task.Start();
                state = State.Request;
            }
        }

        private void ProcessInventoryReply(InventoryReply reply)
        {
            Logging.WriteMessage($"Inventory Received, Promise {reply.InventoryPromiseId}");
            task = new Task(delegate
            {
                Logging.WriteMessage($"Building cache of {reply.Items.Count} market items");

                // Open database (or create if doesn't exist)
                var db = IseCentral.DataCache;
                var marketCache = GetCache(colonyId, CacheType.MarketCache);
                GetCache(colonyId, CacheType.ColonyBasket).DeleteAll();
                db.GetCollection<DBCachedTradable>(Tables.MarketBasket).DeleteAll();
                marketCache.DeleteAll();
                marketCache.InsertBulk(
                    reply.Items.Select(
                        tradable => new DBCachedTradable
                        {
                            ItemCode = tradable.ItemCode,
                            ThingDef = tradable.ThingDef,
                            Quantity = tradable.Quantity,
                            HitPoints = 100,
                            Quality = tradable.Quality,
                            Stuff = tradable.Stuff,
                            Weight = tradable.Weight,
                            WeBuyAt = tradable.WeBuyAt,
                            WeSellAt = tradable.WeSellAt,
                            Minified = tradable.Minified,
                            TranslatedName = DefDatabase<ThingDef>.GetNamed(tradable.ThingDef).LabelCap,
                            TranslatedStuff = tradable.Stuff.NullOrEmpty()
                                ? ""
                                : (string) DefDatabase<ThingDef>.GetNamed(tradable.Stuff).LabelCap,
                            Category = DefDatabase<ThingDef>.GetNamed(tradable.ThingDef).FirstThingCategory.defName
                        }));
                marketCache.EnsureIndex(mc => mc.ThingDef);
                var inventoryCache = db.GetCollection<DBInventoryPromise>(Tables.Promises);
                inventoryCache.DeleteMany(x => x.ColonyId == colonyId);
                inventoryCache.Insert(new DBInventoryPromise
                {
                    ColonyId = colonyId,
                    InventoryPromiseId = reply.InventoryPromiseId,
                    InventoryPromiseExpires = reply.InventoryPromiseExpires,
                    CollectionChargePerKG = reply.CollectionChargePerKG,
                    DeliveryChargePerKG = reply.DeliveryChargePerKG, AccountBalance = reply.AccountBalance
                });
                Logging.WriteMessage("Done caching market items");
            });
            task.Start();

            // Go to next step
            state = State.MarketCaching;
        }

        private void GatherColonyInventory()
        {
            task = new Task(delegate
            {
                Logging.WriteMessage("Building colony item cache");

                Logging.WriteDebugMessage("Getting list of all items in range of beacons");
                Logging.WriteDebugMessage($"on map for {pawn.Name}");

                colonyThings = AllColonyThingsForTrade(pawn.Map);

                // Open database (or create if doesn't exist)
                var db = IseCentral.DataCache;
                var marketCache = GetCache(colonyId, CacheType.MarketCache);
                var colonyCache = GetCache(colonyId, CacheType.ColonyCache);

                // Clear colony cache

                Logging.WriteDebugMessage("Cleared colony cache");

                colonyCache.DeleteAll();
                colonyCache.EnsureIndex(cc => cc.ThingDef);

                // For each downloaded market item
                foreach (var thingGroup in colonyThings.GroupBy(ci => ci.def.defName))
                {
                    Logging.WriteDebugMessage($"Working on group {thingGroup.Key}");

                    foreach (var ci in thingGroup.OrderByDescending(ci => ci.HitPoints))
                    {
                        var unpackedThing = ci.GetInnerIfMinified();

                        var qualityCategory = unpackedThing.TryGetComp<CompQuality>()?.Quality;
                        var stuff = unpackedThing.Stuff?.defName;

                        if (qualityCategory != null && (int) qualityCategory < 2)
                        {
                            Logging.WriteDebugMessage(
                                $"Item {unpackedThing.ThingID} was less than Normal quality, Skipped");

                            continue;
                        }

                        // Check if the item has quality, set to 0 if not.
                        var intQuality = qualityCategory == null ? 0 : (int) qualityCategory;


                        Logging.WriteDebugMessage(
                            $"Searching market for {unpackedThing.def.defName}, " +
                            $"Quality: {intQuality}, " +
                            $"Stuff: {stuff} ");


                        // Get all matching colony items
                        var matchMarketCacheItem = marketCache.FindOne(cacheItem =>
                            cacheItem.ThingDef == unpackedThing.def.defName &&
                            cacheItem.Stuff == stuff &&
                            cacheItem.Quality == intQuality
                        );


                        if (matchMarketCacheItem == null)
                        {
                            if (unpackedThing.def.defName != ThingDefSilver)
                                Logging.WriteMessage(
                                    $"Did not find a Market entry for {unpackedThing.def.defName}, " +
                                    "This could be a problem.");

                            if (!(ci is MinifiedThing))
                                // If not minified skip this ThingDef group, else go to next minified item
                                break;

                            continue;
                        }


                        // Calculate percentage of HP remaining
                        var itemHitPointsAsPercentage = 100;

                        Logging.WriteDebugMessage(
                            $"Item Stack HP {unpackedThing.HitPoints}/{unpackedThing.MaxHitPoints}, " +
                            $"Size: {unpackedThing.stackCount}");

                        if (unpackedThing.def.useHitPoints)
                            // The brackets here are not redundant no matter what Rider/ReSharper suggests
                            // Removing the casts/brackets causes incorrect percentage computation.
                            itemHitPointsAsPercentage = CalculateThingHitPoints(unpackedThing);

                        Logging.WriteDebugMessage($"Item Stack HP {itemHitPointsAsPercentage}%");

                        // Below this threshold, we don't  want the remaining items in this groups
                        // Break now to save iteration.
                        if (itemHitPointsAsPercentage < 15)
                        {
                            Logging.WriteDebugMessage("Item has low HP, skipping.");

                            if (!(ci is MinifiedThing))
                                // If not minified skip this ThingDef group, else go to next minified item
                                break;

                            continue;
                        }

                        var matchColonyCacheItem = colonyCache.FindOne(cacheItem =>
                            unpackedThing.def.defName == cacheItem.ThingDef &&
                            stuff == cacheItem.Stuff &&
                            // Check if the item has quality, set to 0 if not.
                            (qualityCategory == null ? 0 : (int) qualityCategory) == cacheItem.Quality &&
                            itemHitPointsAsPercentage == cacheItem.HitPoints
                        ) ?? new DBCachedTradable
                        {
                            ItemCode = matchMarketCacheItem.ItemCode,
                            ThingDef = matchMarketCacheItem.ThingDef,
                            Quantity = 0,
                            HitPoints = itemHitPointsAsPercentage,
                            Quality = matchMarketCacheItem.Quality,
                            Stuff = matchMarketCacheItem.Stuff,
                            Weight = matchMarketCacheItem.Weight,
                            WeBuyAt = matchMarketCacheItem.WeBuyAt * (itemHitPointsAsPercentage / 100f),
                            WeSellAt = matchMarketCacheItem.WeSellAt * (itemHitPointsAsPercentage / 100f),
                            Minified = matchMarketCacheItem.Minified,
                            Category = matchMarketCacheItem.Category,
                            TranslatedName = matchMarketCacheItem.TranslatedName,
                            TranslatedStuff = matchMarketCacheItem.TranslatedStuff
                        };
                        matchColonyCacheItem.Quantity += unpackedThing.stackCount;
                        colonyCache.Upsert(matchColonyCacheItem);
                    }
                }

                Logging.WriteMessage("Done caching colony items");

                Logging.WriteDebugMessage("Restoring basket");
                RestoreBasket();
                Logging.WriteDebugMessage("Done");
            });
            task.Start();

            // Go to next step
            state = State.ColonyCaching;
        }

        private void RestoreBasket()
        {
            var db = IseCentral.DataCache;
            var marketCache = GetCache(colonyId, CacheType.MarketCache);
            var marketBasket = GetCache(colonyId, CacheType.MarketBasket);

            var colonyCache = GetCache(colonyId, CacheType.ColonyCache);
            var colonyBasket = GetCache(colonyId, CacheType.ColonyBasket);

            marketCache.EnsureIndex(cc => cc.ItemCode);
            colonyCache.EnsureIndex(cc => cc.ItemCode);
            colonyBasket.EnsureIndex(cc => cc.ItemCode);
            marketBasket.EnsureIndex(cc => cc.ItemCode);

            void FindAndSetQuantity(ILiteCollection<DBCachedTradable> collection,
                ILiteCollection<DBCachedTradable> basket)
            {
                foreach (var tradable in collection.FindAll())
                {
                    // Get matching basket item
                    var matchBasketItem = basket.FindById(tradable.ItemCode);

                    // We didn't have a matching item, go to next.
                    if (matchBasketItem == null) continue;

                    // Try and set quantity to the what it was before, but clamp it to the new max value if needed
                    tradable.TradedQuantity = Mathf.Clamp(
                        matchBasketItem.TradedQuantity,
                        0,
                        tradable.Quantity
                    );
                    collection.Upsert(tradable);
                }
            }

            FindAndSetQuantity(colonyCache, colonyBasket);
            FindAndSetQuantity(marketCache, marketBasket);

            // Delete any items in the basket which we don't have anymore
            var cachedItems = colonyCache.FindAll().Select(cc => cc.ItemCode);
            colonyBasket.DeleteMany(x => !cachedItems.Contains(x.ItemCode));

            // Delete any items in the basket which we don't have anymore
            var marketCachedItems = marketCache.FindAll().Select(cc => cc.ItemCode);
            marketBasket.DeleteMany(x => !marketCachedItems.Contains(x.ItemCode));
        }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Request,
            MarketCaching,
            ColonyCaching,
            Done,
            Error,
        }

        #endregion
    }
}