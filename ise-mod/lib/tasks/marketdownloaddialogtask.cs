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
using System.Threading;
using System.Threading.Tasks;
using Inventory;
using ise.components;
using ise.dialogs;
using ise_core.db;
using ise_core.extend;
using LiteDB;
using RimWorld;
using Tradable;
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

        public MarketDownloadDialogTask(IDialog dialog, Pawn userPawn, bool firstLoad = true,
            ThingCategoryDef thingCategoryDef = null) : base(dialog)
        {
            _pawn = userPawn;
            _state = State.Start;
            _gc = Current.Game.GetComponent<ISEGameComponent>();
            if (_pawn == null) throw new ArgumentNullException(nameof(userPawn));
            _thingCategoryDef = thingCategoryDef;
            _firstLoad = firstLoad;
        }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            RequestInventory,
            UpdateTradables,
            RequestingPromise,
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
        private readonly ThingCategoryDef _thingCategoryDef;
        private readonly bool _firstLoad;
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
                    if (_task == null) MarketVersionLogic();
                    break;

                case State.UpdateTradables:
                    Dialog.DialogMessage = "Uploading item list";
                    if (_task != null && _task.IsCompleted)
                        ProcessColonyTradablesReply(((Task<bool>)_task).Result);
                    break;

                // case State.RequestingPromise:
                //     Dialog.DialogMessage = "Requesting Server Attention";
                //     if (_task != null && _task.IsCompleted)
                //         ProcessGeneratePromiseReply(((Task<GeneratePromiseReply>)_task).Result);
                //     break;

                case State.RequestInventory:
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
            Logging.LoggerInstance.WriteErrorMessage($"Unhandled exception in task {_task.Exception}");
            if (_task.Exception?.InnerExceptions != null)
                foreach (var innerException in _task.Exception.InnerExceptions)
                    Logging.LoggerInstance.WriteErrorMessage($"Inner exception in task {innerException}");

            _task = null;
        }

        private void MarketVersionLogic()
        {
#if MARKET_V2
            StartColonyUpdateTradables();
#else
                StartDownload();
#endif
        }


        private void StartDownload()
        {
            var db = IseCentral.DataCache;
            var inventoryPromise = db.GetCollection<DBInventoryPromise>(Tables.Promises).FindById(_colonyId);

            // TODO: Fix this, check with server if promise is valid, it might've been deleted
            if (inventoryPromise != null && inventoryPromise.InventoryPromiseExpires > GetUTCNow())
            {
                _promiseID = inventoryPromise.InventoryPromiseId;

                // Thing category def is only ever null in the V1 market code,
                // In the V1 version we just need to get the colony inventory,
                // In V2, we only need to do this on the first load 
                if (_thingCategoryDef == null)
                {
                    GatherColonyInventory();
                    return;
                }
            }

            _task = ise_core.rest.api.v1.Inventory.GetInventoryAsync(_gc.ClientBind, _colonyId, !_firstLoad);
            _task.Start();
            _state = State.RequestInventory;
        }

        private const int TradableBatchSize = 25_000;

        private void StartColonyUpdateTradables()
        {
            Logging.LoggerInstance.WriteDebugMessage(
                $"UpdateAsync Colony tradables {_colonyId} for category {_thingCategoryDef.defName}");

            _task = new Task<bool>(() =>
            {
                var awaitTasks = new List<Task<bool>>();

                var tradables = GetAllTradables(_thingCategoryDef).ToList();

                if (!_firstLoad)
                {
                    // We also need to add any items that are in the basket
                    // otherwise it will break when the cache is cleared
                    tradables.AddRange(GetTradablesFromBasket());
                }

                var itemsSent = 0;
                foreach (var batch in tradables.Batch(TradableBatchSize))
                {
                    var itemsToSend = batch.ToList();
                    itemsSent += itemsToSend.Count;
                    var finalPacket = itemsSent == tradables.Count;

                    Logging.LoggerInstance.WriteDebugMessage(
                        $"Sending {itemsToSend.Count} tradables, final packet: {finalPacket}");

                    if (finalPacket)
                    {
                        if (awaitTasks.Count > 0)
                        {
                            Logging.LoggerInstance.WriteDebugMessage(
                                $"Final packet waiting for {awaitTasks.Count} other requests to finish");
                            while (awaitTasks.Select(t => t.IsCompleted).Any(s => !s)) Thread.Sleep(10);
                        }

                        Logging.LoggerInstance.WriteDebugMessage("Sending final packet");
                    }

                    var batchTask = new Task<bool>(() => ise_core.rest.api.v1.Colony.SetTradablesList(
                        _gc.ClientBind,
                        _colonyId,
                        itemsToSend,
                        finalPacket
                    ));
                    batchTask.Start();
                    awaitTasks.Add(batchTask);
                }

                Logging.LoggerInstance.WriteDebugMessage("Waiting for final request to finish");
                while (awaitTasks.Select(t => t.IsCompleted).Any(s => !s)) Thread.Sleep(10);
                return awaitTasks.All(t => t.Result);
            });

            _state = State.UpdateTradables;
            _task.Start();
        }

        private void ProcessColonyTradablesReply(bool reply)
        {
            if (!reply)
            {
                _state = State.Error;
                Logging.LoggerInstance.WriteErrorMessage("Server did not accept colony tradables");
            }

            Logging.LoggerInstance.WriteDebugMessage("Server accepted colony tradables");

            // If we already have a promise, go straight to download.
            StartDownload();
        }

        // private void ProcessGeneratePromiseReply(GeneratePromiseReply reply)
        // {
        //     Logging.LoggerInstance.WriteDebugMessage($"Inventory Received, Promise {reply.InventoryPromiseId}");
        //     _promiseID = reply.InventoryPromiseId;
        //
        //     var db = IseCentral.DataCache;
        //     try
        //     {
        //         db.BeginTrans();
        //         var inventoryCache = db.GetCollection<DBInventoryPromise>(Tables.Promises);
        //         inventoryCache.DeleteMany(x => x.ColonyId == _colonyId);
        //         inventoryCache.Insert(new DBInventoryPromise
        //         {
        //             ColonyId = _colonyId,
        //             InventoryPromiseId = reply.InventoryPromiseId,
        //             InventoryPromiseExpires = reply.InventoryPromiseExpires,
        //         });
        //         db.Commit();
        //     }
        //     catch (Exception)
        //     {
        //         db.Rollback();
        //         throw;
        //     }
        //
        //     StartDownload();
        // }

        private void ProcessInventoryReply(InventoryReply reply)
        {
            Logging.LoggerInstance.WriteDebugMessage($"Inventory Received, Promise {reply.InventoryPromiseId}");
            _promiseID = reply.InventoryPromiseId;
            _task = new Task(delegate
            {
                Logging.LoggerInstance.WriteDebugMessage($"Building cache of {reply.Items.Count} market items");

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
                Logging.LoggerInstance.WriteDebugMessage($"Received {reply.Items.Count}, Wrote {writeCount}");
                Logging.LoggerInstance.WriteDebugMessage("Done caching market items");
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
                Logging.LoggerInstance.WriteDebugMessage($"thingDef {tradable.ThingDef} has no category, won't be tradable");
                return null;
            }

            Logging.LoggerInstance.WriteDebugMessage($"thingDef {tradable.ThingDef} " +
                                      $"{(tradable.Stuff.NullOrEmpty() ? "or " + tradable.Stuff : "")} " +
                                      "has no label or couldn't be found, won't be tradable");
            return null;
        }

        private void GatherColonyInventory()
        {
            _task = new Task(delegate
            {
                Logging.LoggerInstance.WriteDebugMessage("Building colony item cache");

                Logging.LoggerInstance.WriteDebugMessage("Getting list of all items in range of beacons");
                Logging.LoggerInstance.WriteDebugMessage($"on map for {_pawn.Name}");

                _colonyThings = AllColonyThingsForTrade(_pawn.Map);

                // Open database (or create if doesn't exist)
                var db = IseCentral.DataCache;
                var marketCache = GetCache(_colonyId, CacheType.MarketCache);
                var colonyCache = GetCache(_colonyId, CacheType.ColonyCache);

                // Clear colony cache

                Logging.LoggerInstance.WriteDebugMessage("Cleared colony cache");

                colonyCache.DeleteAll();
                colonyCache.EnsureIndex(cc => cc.ThingDef);
                colonyCache.EnsureIndex(cc => cc.IndexedName);

                // For each downloaded market item
                foreach (var thingGroup in _colonyThings.GroupBy(ci => ci.def.defName))
                {
                    Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                        $"Working on group {thingGroup.Key}");


                    foreach (var ci in thingGroup.OrderByDescending(ci => ci.HitPoints))
                    {
                        var unpackedThing = ci.GetInnerIfMinified();

                        var qualityCategory = unpackedThing.TryGetComp<CompQuality>()?.Quality;
                        var stuff = unpackedThing.Stuff?.defName;
                        var stuffString = stuff.NullOrEmpty() ? "null" : stuff;
                        if (qualityCategory != null && (int)qualityCategory < 2)
                        {
                            Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                                $"Item {unpackedThing.ThingID} was less than Normal quality, Skipped");

                            continue;
                        }

                        // Check if the item has quality, set to 0 if not.
                        var intQuality = qualityCategory == null ? 0 : (int)qualityCategory;


                        Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
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
                                Logging.LoggerInstance.WriteDebugMessage(
                                    $"Did not find a Market entry for {unpackedThing.def.defName}, " +
                                    "This could be a problem.");

                            if (!(ci is MinifiedThing))
                                // If not minified skip this ThingDef group, else go to next minified item
                                break;

                            continue;
                        }

                        // Calculate percentage of HP remaining
                        var itemHitPointsAsPercentage = 100;

                        Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"Item Stack HP {unpackedThing.HitPoints}/{unpackedThing.MaxHitPoints}, " +
                            $"Size: {unpackedThing.stackCount}");


                        if (unpackedThing.def.useHitPoints)
                            // The brackets here are not redundant no matter what Rider/ReSharper suggests
                            // Removing the casts/brackets causes incorrect percentage computation.
                            itemHitPointsAsPercentage = CalculateThingHitPoints(unpackedThing);

                        Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"Item Stack HP {itemHitPointsAsPercentage}%");

                        // Below this threshold, we don't  want the remaining items in this groups
                        // Break now to save iteration.
                        if (itemHitPointsAsPercentage < 15)
                        {
                            Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                                "Item has low HP, skipping.");

                            if (!(ci is MinifiedThing))
                                // If not minified skip this ThingDef group, else go to next minified item
                                break;

                            continue;
                        }

                        Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
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
                        Logging.LoggerInstance.WriteDebugMessage(IseCentral.Settings.DebugTradeBeacons,
                            $"New quantity is {matchColonyCacheItem.Quantity}");
                        colonyCache.Upsert(matchColonyCacheItem);
                    }
                }

                Logging.LoggerInstance.WriteDebugMessage("Done caching colony items");

                Logging.LoggerInstance.WriteDebugMessage("Restoring basket");
                RestoreBasket();
                Logging.LoggerInstance.WriteDebugMessage("Done");
            });
            _task.Start();

            // Go to next step
            _state = State.ColonyCaching;
        }

        private IEnumerable<ColonyTradable> GetTradablesFromBasket()
        {
            var marketBasket = GetCache(_colonyId, CacheType.MarketBasket);
            var colonyBasket = GetCache(_colonyId, CacheType.ColonyBasket);

            var tradables = marketBasket.FindAll().Select(item => CacheToTradable(item)).ToList();
            tradables.AddRange(colonyBasket.FindAll().Select(item => CacheToTradable(item)));

            return tradables;
        }

        private ColonyTradable CacheToTradable(DBCachedTradable item)
        {
            return new ColonyTradable
            {
                ThingDef = item.ThingDef,
                Quality = item.Quantity,
                Minified = item.Minified,
                BaseValue = item.BaseValue,
                Weight = item.Weight,
                Stuff = item.Stuff
            };
        }

        private void RestoreBasket()
        {
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
            if (_thingCategoryDef == null || _firstLoad)
            {
                Logging.LoggerInstance.WriteDebugMessage($"Activating Promise {_promiseID}");
                _task = ise_core.rest.api.v1.Inventory.ActivatePromiseAsync(_gc.ClientBind, _colonyId, _promiseID);
                _task.Start();
                _state = State.ActivatePromise;
            }
            else
            {
                // We only need to activate the promise once
                _state = State.Done;
            }
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