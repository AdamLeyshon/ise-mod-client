#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, client_bind.cs, Created 2021-02-11

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
using static ise.lib.Consts;
using static ise_core.rest.api.v1.consts;
using static ise.lib.Tradables;

namespace ise.lib.tasks
{
    internal class MarketDownloadDialogTask : AbstractDialogTask
    {
        private enum State
        {
            Start,
            Request,
            MarketCaching,
            ColonyCaching,
            Done,
            Error,
        }

        private State state;
        private Task task;
        private List<Thing> colonyThings;
        private readonly Pawn pawn;

        public MarketDownloadDialogTask(IDialog dialog, Pawn userPawn) : base(dialog)
        {
            pawn = userPawn;
            state = State.Start;

            if (pawn == null)
            {
                Logging.WriteErrorMessage("Pawn is NULL!");
            }
        }

        public override void Update()
        {
            // Handle task errors first.
            if (task != null && task.IsFaulted)
            {
                LogTaskError();
            }

            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartDownload();
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Downloading Market Data";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessInventoryReply(((Task<InventoryReply>) task).Result);
                    }

                    break;
                case State.MarketCaching:
                    Dialog.DialogMessage = "Building Cache";
                    if (task != null && task.IsCompleted)
                    {
                        GatherColonyInventory();
                    }

                    break;
                case State.ColonyCaching:
                    Dialog.DialogMessage = "Getting colony inventory";
                    if (task != null && task.IsCompleted)
                    {
                        state = State.Done;
                    }

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
                {
                    Logging.WriteErrorMessage($"Inner exception in task {innerException}");
                }

            task = null;
        }

        private void StartDownload()
        {
            DBInventory inventory;
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                inventory = db.GetCollection<DBInventory>().FindAll().FirstOrDefault();
            }

            if (inventory != null && inventory.InventoryPromiseExpires > GetUTCNow())
            {
                // No need to download, promise is still valid
                GatherColonyInventory();
            }
            else
            {
                var gc = Current.Game.GetComponent<ISEGameComponent>();
                var request = new InventoryRequest {ColonyId = gc.ColonyBind, ClientBindId = gc.ClientBind};
                Logging.WriteMessage($"Asking server for Inventory");
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

                var thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;

                // Open database (or create if doesn't exist)
                using (var db = new LiteDatabase(DBLocation))
                {
                    var marketCache = db.GetCollection<DBCachedTradable>("market_cache");
                    db.GetCollection<DBCachedTradable>("colony_basket").DeleteAll();
                    db.GetCollection<DBCachedTradable>("market_basket").DeleteAll();
                    marketCache.DeleteAll();
                    marketCache.InsertBulk(
                        reply.Items.Select(
                            tradable => new DBCachedTradable
                            {
                                ItemCode = tradable.ItemCode,
                                ThingDef = tradable.ThingDef,
                                AvailableQuantity = tradable.Quantity,
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
                    var inventoryCache = db.GetCollection<DBInventory>();
                    inventoryCache.DeleteAll();
                    inventoryCache.Insert(new DBInventory
                    {
                        InventoryPromiseId = reply.InventoryPromiseId,
                        InventoryPromiseExpires = reply.InventoryPromiseExpires,
                        CollectionChargePerKG = reply.CollectionChargePerKG,
                        DeliveryChargePerKG = reply.DeliveryChargePerKG, AccountBalance = reply.AccountBalance
                    });
                    Logging.WriteMessage($"Done caching market items");
                }
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
#if MARKET_DEBUG
                Logging.WriteMessage("Getting list of all items in range of beacons");
                Logging.WriteMessage($"on map for {pawn.Name}");
#endif
                colonyThings = AllColonyThingsForTrade(pawn.Map);

                // Open database (or create if doesn't exist)
                using (var db = new LiteDatabase(DBLocation))
                {
                    var marketCache = db.GetCollection<DBCachedTradable>("market_cache");
                    var colonyCache = db.GetCollection<DBCachedTradable>("colony_cache");

                    // Clear colony cache
#if MARKET_DEBUG
                    Logging.WriteMessage("Cleared colony cache");
#endif
                    colonyCache.DeleteAll();
                    colonyCache.EnsureIndex(cc => cc.ThingDef);

                    // For each downloaded market item
                    foreach (var thingGroup in colonyThings.GroupBy(ci => ci.def.defName))
                    {
#if MARKET_DEBUG
                        Logging.WriteMessage($"Working on group {thingGroup.Key}");
#endif
                        foreach (var ci in thingGroup.OrderByDescending(ci => ci.HitPoints))
                        {
                            var unpackedThing = ci.GetInnerIfMinified();

                            var qualityCategory = unpackedThing.TryGetComp<CompQuality>()?.Quality;
                            var stuff = unpackedThing.Stuff?.defName;

                            if (qualityCategory != null && (int) qualityCategory < 2)
                            {
#if MARKET_DEBUG
                                Logging.WriteMessage(
                                    $"Item {unpackedThing.ThingID} was less than Normal quality, Skipped");
#endif
                                continue;
                            }

                            // Check if the item has quality, set to 0 if not.
                            var intQuality = (qualityCategory == null ? 0 : (int) qualityCategory);

#if MARKET_DEBUG
                            Logging.WriteMessage(
                                $"Searching market for {unpackedThing.def.defName}, " +
                                $"Quality: {intQuality}, " +
                                $"Stuff: {stuff} ");
#endif

                            // Get all matching colony items
                            var matchMarketCacheItem = marketCache.FindOne(cacheItem =>
                                cacheItem.ThingDef == unpackedThing.def.defName &&
                                cacheItem.Stuff == stuff &&
                                cacheItem.Quality == intQuality
                            );


                            if (matchMarketCacheItem == null)
                            {
#if MARKET_DEBUG
                                Logging.WriteMessage($"Did not find a Market entry! How?");
#endif
                                if (!(ci is MinifiedThing))
                                {
                                    // If not minified skip this ThingDef group, else go to next minified item
                                    break;
                                }

                                continue;
                            }


                            // Calculate percentage of HP remaining
                            var itemHitPointsAsPercentage = 100;
#if MARKET_DEBUG
                            Logging.WriteMessage(
                                $"Item Stack HP {unpackedThing.HitPoints}/{unpackedThing.MaxHitPoints}, " +
                                $"Size: {unpackedThing.stackCount}");
#endif
                            // -1 is no damage, Ludeon, why? why not just HitPoints=MaxHitPoints?
                            if (unpackedThing.HitPoints != -1)
                            {
                                // The brackets here are not redundant no matter what Rider/ReSharper suggests
                                // Removing the casts/brackets causes incorrect percentage computation.
                                itemHitPointsAsPercentage =
                                    (int) Math.Floor(((float) unpackedThing.HitPoints /
                                                      (float) unpackedThing.MaxHitPoints) * 100f);
                            }
#if MARKET_DEBUG
                            Logging.WriteMessage($"Item Stack HP {itemHitPointsAsPercentage}%");
#endif
                            // Below this threshold, we don't  want the remaining items in this groups
                            // Break now to save iteration.
                            if (itemHitPointsAsPercentage < 15)
                            {
#if MARKET_DEBUG
                                Logging.WriteMessage($"Item has low HP, skipping.");
#endif
                                if (!(ci is MinifiedThing))
                                {
                                    // If not minified skip this ThingDef group, else go to next minified item
                                    break;
                                }

                                continue;
                            }

                            var matchColonyCacheItem = colonyCache.FindOne(cacheItem =>
                                unpackedThing.def.defName == cacheItem.ThingDef &&
                                stuff == cacheItem.Stuff &&
                                // Check if the item has quality, set to 0 if not.
                                (qualityCategory == null ? 0 : (int) qualityCategory) == cacheItem.Quality &&
                                itemHitPointsAsPercentage == cacheItem.HitPoints
                            ) ?? new DBCachedTradable()
                            {
                                ItemCode = matchMarketCacheItem.ItemCode,
                                ThingDef = matchMarketCacheItem.ThingDef,
                                AvailableQuantity = 0,
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
                            matchColonyCacheItem.AvailableQuantity += unpackedThing.stackCount;
                            colonyCache.Upsert(matchColonyCacheItem);
                        }
                    }

                    Logging.WriteMessage($"Done caching colony items");

                    Logging.WriteMessage("Restoring basket");
                    RestoreBasket();
                    Logging.WriteMessage("Done");
                }
            });
            task.Start();

            // Go to next step
            state = State.ColonyCaching;
        }

        private static void RestoreBasket()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                var marketCache = db.GetCollection<DBCachedTradable>("market_cache");
                var colonyCache = db.GetCollection<DBCachedTradable>("colony_cache");
                var colonyBasket = db.GetCollection<DBCachedTradable>("colony_basket");
                var marketBasket = db.GetCollection<DBCachedTradable>("market_basket");
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
                            tradable.AvailableQuantity
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
        }
    }
}