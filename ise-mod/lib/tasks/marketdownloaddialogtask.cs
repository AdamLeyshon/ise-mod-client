#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, marketdownloaddialogtask.cs 2021-02-12

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory;
using ise.components;
using ise.dialogs;
using ise_core.db;
using ise_core.rest;
using LiteDB;
using RestSharp;
using RimWorld;
using UnityEngine;
using Verse;
using static ise_core.rest.Helpers;
using static ise.lib.Constants;
using static ise.lib.Tradables;
using static ise.lib.Cache;

namespace ise.lib.tasks
{
    internal class MarketDownloadDialogTask : AbstractDialogTask
    {
        #region ctor

        public MarketDownloadDialogTask(IDialog dialog, Pawn userPawn) : base(dialog)
        {
            _pawn = userPawn;
            _state = State.Start;
            _gc = Current.Game.GetComponent<ISEGameComponent>();
            if (_pawn == null) throw new ArgumentNullException(nameof(userPawn));
        }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Request,
            MarketCaching,
            ColonyCaching,
            ActivatePromise,
            Done,
            Error
        }

        #endregion

        #region Fields

        private string _colonyId;
        private readonly ISEGameComponent _gc;
        private readonly Pawn _pawn;
        private List<Thing> _colonyThings;
        private string _promiseID;

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

                    // Don't read the Colony ID until we try and talk to the server
                    // It may have changed or just been created in the bind task
                    _colonyId = _gc.GetColonyId(_pawn.Map);
                    if (_task == null) StartDownload();
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Downloading Market Data";
                    if (_task != null && _task.IsCompleted) ProcessInventoryReply(((Task<InventoryReply>)_task).Result);

                    break;
                case State.MarketCaching:
                    Dialog.DialogMessage = "Building Cache";
                    if (_task != null && _task.IsCompleted) GatherColonyInventory();

                    break;
                case State.ColonyCaching:
                    Dialog.DialogMessage = "Getting colony inventory";
                    if (_task != null && _task.IsCompleted) StartActivatePromise();

                    break;
                case State.ActivatePromise:
                    Dialog.DialogMessage = "Completing transaction on Server";
                    if (_task != null && _task.IsCompleted)
                        ProcessActivatePromiseReply(((Task<ActivatePromiseReply>)_task).Result);

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
            _state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {_task.Exception}");
            if (_task.Exception?.InnerExceptions != null)
                foreach (var innerException in _task.Exception.InnerExceptions)
                    Logging.WriteErrorMessage($"Inner exception in task {innerException}");

            _task = null;
        }

        private void StartDownload()
        {
            var db = IseCentral.DataCache;
            var inventoryPromise = db.GetCollection<DBInventoryPromise>(Tables.Promises).FindById(_colonyId);

            // TODO: Fix this, check with server if promise is valid, it might've been deleted
            if (inventoryPromise != null && inventoryPromise.InventoryPromiseExpires > GetUTCNow())
            {
                _promiseID = inventoryPromise.InventoryPromiseId;
                // No need to download, promise is still valid.
                GatherColonyInventory();
                return;
            }

            _task = ise_core.rest.api.v1.Inventory.GetInventoryAsync(_gc.ClientBind, _colonyId);
            _task.Start();
            _state = State.Request;
        }

        private void ProcessInventoryReply(InventoryReply reply)
        {
            Logging.WriteDebugMessage($"Inventory Received, Promise {reply.InventoryPromiseId}");
            _promiseID = reply.InventoryPromiseId;
            _task = new Task(delegate
            {
                Logging.WriteDebugMessage($"Building cache of {reply.Items.Count} market items");

                // Open database (or create if doesn't exist)
                var db = IseCentral.DataCache;

                try
                {
                    db.BeginTrans();
                    var marketCache = GetCache(_colonyId, CacheType.MarketCache);
                    GetCache(_colonyId, CacheType.ColonyBasket).DeleteAll();
                    db.GetCollection<DBCachedTradable>(Tables.MarketBasket).DeleteAll();
                    marketCache.DeleteAll();
                    marketCache.InsertBulk(
                        reply.Items.Select(MakeDBRowFromTradable).Where(x => x != null));
                    marketCache.EnsureIndex(mc => mc.ThingDef);
                    marketCache.EnsureIndex(mc => mc.IndexedName);
                    var inventoryCache = db.GetCollection<DBInventoryPromise>(Tables.Promises);
                    inventoryCache.DeleteMany(x => x.ColonyId == _colonyId);
                    inventoryCache.Insert(new DBInventoryPromise
                    {
                        ColonyId = _colonyId,
                        InventoryPromiseId = reply.InventoryPromiseId,
                        InventoryPromiseExpires = reply.InventoryPromiseExpires,
                        CollectionChargePerKG = reply.CollectionChargePerKG,
                        DeliveryChargePerKG = reply.DeliveryChargePerKG, AccountBalance = reply.AccountBalance
                    });

                    db.Commit();
                }
                catch (Exception)
                {
                    db.Rollback();
                    throw;
                }

                var writeCount = GetCache(_colonyId, CacheType.MarketCache).Count();
                Logging.WriteDebugMessage($"Received {reply.Items.Count}, Wrote {writeCount}");
                Logging.WriteDebugMessage("Done caching market items");
            });
            _task.Start();

            // Go to next step
            _state = State.MarketCaching;
        }

        private DBCachedTradable MakeDBRowFromTradable(Tradable.Tradable tradable)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamed(tradable.ThingDef, false);
            var stuffDef = DefDatabase<ThingDef>.GetNamed(tradable.Stuff, false);

            if (thingDef != null && !thingDef.LabelCap.NullOrEmpty() &&
                (stuffDef == null || !stuffDef.LabelCap.NullOrEmpty()))
            {
                if (thingDef.FirstThingCategory != null)
                    return new DBCachedTradable
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
                        TranslatedName = thingDef.LabelCap,
                        IndexedName = thingDef.LabelCap.ToLower(),
                        TranslatedStuff = stuffDef != null ? (string)stuffDef.LabelCap : "",
                        Category = thingDef.FirstThingCategory.defName
                    };
                Logging.WriteDebugMessage($"thingDef {tradable.ThingDef} has no category, won't be tradable");
                return null;
            }

            Logging.WriteDebugMessage($"thingDef {tradable.ThingDef} " +
                                      $"{(tradable.Stuff.NullOrEmpty() ? "or " + tradable.Stuff : "")} " +
                                      "has no label or couldn't be found, won't be tradable");
            return null;
        }

        private void GatherColonyInventory()
        {
            _task = new Task(delegate
            {
                Logging.WriteDebugMessage("Building colony item cache");

                Logging.WriteDebugMessage("Getting list of all items in range of beacons");
                Logging.WriteDebugMessage($"on map for {_pawn.Name}");

                _colonyThings = AllColonyThingsForTrade(_pawn.Map);

                // Open database (or create if doesn't exist)
                var db = IseCentral.DataCache;
                var marketCache = GetCache(_colonyId, CacheType.MarketCache);
                var colonyCache = GetCache(_colonyId, CacheType.ColonyCache);

                // Clear colony cache

                Logging.WriteDebugMessage("Cleared colony cache");

                colonyCache.DeleteAll();
                colonyCache.EnsureIndex(cc => cc.ThingDef);
                colonyCache.EnsureIndex(cc => cc.IndexedName);

                // For each downloaded market item
                foreach (var thingGroup in _colonyThings.GroupBy(ci => ci.def.defName))
                {
                    Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                        $"Working on group {thingGroup.Key}");


                    foreach (var ci in thingGroup.OrderByDescending(ci => ci.HitPoints))
                    {
                        var unpackedThing = ci.GetInnerIfMinified();

                        var qualityCategory = unpackedThing.TryGetComp<CompQuality>()?.Quality;
                        var stuff = unpackedThing.Stuff?.defName;
                        var stuffString = stuff.NullOrEmpty() ? "null" : stuff;
                        if (qualityCategory != null && (int)qualityCategory < 2)
                        {
                            Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                                $"Item {unpackedThing.ThingID} was less than Normal quality, Skipped");

                            continue;
                        }

                        // Check if the item has quality, set to 0 if not.
                        var intQuality = qualityCategory == null ? 0 : (int)qualityCategory;


                        Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"Searching market for {unpackedThing.def.defName}, " +
                            $"Quality: {intQuality}, " +
                            $"Stuff: {stuffString} ");


                        // Get all matching colony items
                        var matchMarketCacheItem = marketCache.FindOne(cacheItem =>
                            cacheItem.ThingDef == unpackedThing.def.defName &&
                            cacheItem.Stuff == stuff &&
                            cacheItem.Quality == intQuality
                        );


                        if (matchMarketCacheItem == null)
                        {
                            if (unpackedThing.def.defName != ThingDefSilver)
                                Logging.WriteDebugMessage(
                                    $"Did not find a Market entry for {unpackedThing.def.defName}, " +
                                    "This could be a problem.");

                            if (!(ci is MinifiedThing))
                                // If not minified skip this ThingDef group, else go to next minified item
                                break;

                            continue;
                        }

                        // Calculate percentage of HP remaining
                        var itemHitPointsAsPercentage = 100;

                        Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"Item Stack HP {unpackedThing.HitPoints}/{unpackedThing.MaxHitPoints}, " +
                            $"Size: {unpackedThing.stackCount}");


                        if (unpackedThing.def.useHitPoints)
                            // The brackets here are not redundant no matter what Rider/ReSharper suggests
                            // Removing the casts/brackets causes incorrect percentage computation.
                            itemHitPointsAsPercentage = CalculateThingHitPoints(unpackedThing);

                        Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"Item Stack HP {itemHitPointsAsPercentage}%");

                        // Below this threshold, we don't  want the remaining items in this groups
                        // Break now to save iteration.
                        if (itemHitPointsAsPercentage < 15)
                        {
                            Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                                "Item has low HP, skipping.");

                            if (!(ci is MinifiedThing))
                                // If not minified skip this ThingDef group, else go to next minified item
                                break;

                            continue;
                        }

                        Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"Searching colony cache for " +
                            $"cacheItem.ThingDef = {unpackedThing.def.defName}, " +
                            $"cacheItem.Stuff == {stuffString}, " +
                            $"cacheItem.Quality == {intQuality}, " +
                            $"cacheItem.HitPoints == {itemHitPointsAsPercentage}");

                        var matchColonyCacheItem = colonyCache.FindOne(cacheItem =>
                            unpackedThing.def.defName == cacheItem.ThingDef &&
                            stuff == cacheItem.Stuff &&
                            intQuality == cacheItem.Quality &&
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
                            IndexedName = matchMarketCacheItem.IndexedName,
                            TranslatedStuff = matchMarketCacheItem.TranslatedStuff
                        };

                        matchColonyCacheItem.Quantity += unpackedThing.stackCount;
                        Logging.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"New quantity is {matchColonyCacheItem.Quantity}");
                        colonyCache.Upsert(matchColonyCacheItem);
                    }
                }

                Logging.WriteDebugMessage("Done caching colony items");

                Logging.WriteDebugMessage("Restoring basket");
                RestoreBasket();
                Logging.WriteDebugMessage("Done");
            });
            _task.Start();

            // Go to next step
            _state = State.ColonyCaching;
        }

        private void RestoreBasket()
        {
            var db = IseCentral.DataCache;
            var marketCache = GetCache(_colonyId, CacheType.MarketCache);
            var marketBasket = GetCache(_colonyId, CacheType.MarketBasket);

            var colonyCache = GetCache(_colonyId, CacheType.ColonyCache);
            var colonyBasket = GetCache(_colonyId, CacheType.ColonyBasket);

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

        private void StartActivatePromise()
        {
            Logging.WriteDebugMessage($"Activating Promise {_promiseID}");
            _task = ise_core.rest.api.v1.Inventory.ActivatePromiseAsync(_gc.ClientBind, _colonyId, _promiseID);
            _task.Start();
            _state = State.ActivatePromise;
        }

        private void ProcessActivatePromiseReply(ActivatePromiseReply reply)
        {
            if (reply.Success)
            {
                var db = IseCentral.DataCache;
                var inventoryPromise = db.GetCollection<DBInventoryPromise>(Tables.Promises).FindById(_colonyId);
                inventoryPromise.InventoryPromiseExpires = reply.InventoryPromiseExpires;
                var inventoryCache = db.GetCollection<DBInventoryPromise>(Tables.Promises);
                inventoryCache.Update(inventoryPromise);
            }

            // Go to next step
            _state = State.Done;
        }

        #endregion
    }
}