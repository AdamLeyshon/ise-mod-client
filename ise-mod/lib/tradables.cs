#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, tradables.cs 2021-02-03

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Tradable;
using Verse;

namespace ise.lib
{
    internal static class Tradables
    {
        #region Methods

        internal static IEnumerable<ColonyTradable> GetAllTradables()
        {
            var thingsToRequestFromMarket = new List<ColonyTradable>();
            var stuffCategoryDefs = DefDatabase<StuffCategoryDef>.AllDefsListForReading;
            var things = DefDatabase<ThingDef>.AllDefsListForReading;

            var stuffCategories = stuffCategoryDefs.ToDictionary(
                stuffCategory => stuffCategory,
                stuffCategory => new List<ThingDef>()
            );

            // Scan Defs for Things that can be used as stuff.
            foreach (var thing in things)
                // If this can be used as Stuff and it has StuffCategories assigned
                if (thing.IsStuff && thing.stuffProps.categories.Count > 0)
                    foreach (var category in thing.stuffProps.categories)
                        stuffCategories[category].Add(thing);

            // Then scan for Defs that can be traded.
            foreach (var thing in things)
                try
                {
                    if (thing.tradeability != Tradeability.None &&
                        thing.FirstThingCategory != null &&
                        thing.GetStatValueAbstract(StatDefOf.MarketValue) > 0.0 &&
                        (thing.category == ThingCategory.Item || thing.category == ThingCategory.Building &&
                            thing.Minifiable))

                        thingsToRequestFromMarket.AddRange(ComputeThingDef(thing, stuffCategories));
                }
                catch (Exception ex)
                {
                    Logging.WriteErrorMessage(
                        $"failed to process ThingDef {thing.defName} for trading, it won't be tradeable.");
                    Logging.WriteErrorMessage(ex.Message);
                }

            return thingsToRequestFromMarket;
        }


        internal static float GetValueForThing(string thing, int quality, [CanBeNull] string stuff)
        {
            var thingDef = DefDatabase<ThingDef>.GetNamed(thing);
            var stuffDef = stuff.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamed(stuff);
            return StatDefOf.MarketValue.Worker.GetValue(quality > 2
                ? StatRequest.For(thingDef, stuffDef, (QualityCategory)quality)
                : StatRequest.For(thingDef, stuffDef));
        }

        private static IEnumerable<ColonyTradable> ComputeItemsMadeFromStuff(
            ThingDef thing, IReadOnlyDictionary<StuffCategoryDef, List<ThingDef>> stuffData
        )
        {
            foreach (var stuffThings in thing.stuffCategories.Select(category => stuffData[category]))
            {
                foreach (var stuff in stuffThings)
                    yield return new ColonyTradable
                    {
                        ThingDef = thing.defName,
                        Stuff = stuff.defName,
                        Minified = thing.Minifiable,
                        Quality = -1,
                        BaseValue = (float)Math.Round(StatDefOf.MarketValue.Worker
                            .GetValue(StatRequest.For(thing, stuff)), 2, MidpointRounding.AwayFromZero),
                        Weight = StatDefOf.Mass.Worker.GetValue(StatRequest.For(thing, stuff))
                    };
                ;
            }
        }

        private static IEnumerable<ColonyTradable> ComputeItemsMadeFromStuffWithQuality(ThingDef thing,
            IReadOnlyDictionary<StuffCategoryDef, List<ThingDef>> stuffData)
        {
            for (var quality = 2; quality < 7; quality++)
                foreach (var category in thing.stuffCategories)
                {
                    var stuffThings = stuffData[category];
                    foreach (var stuff in stuffThings)
                        yield return new ColonyTradable
                        {
                            ThingDef = thing.defName,
                            Stuff = stuff.defName,
                            Minified = thing.Minifiable,
                            Quality = quality,
                            BaseValue = (float)Math.Round(StatDefOf.MarketValue.Worker
                                    .GetValue(StatRequest.For(thing, stuff, (QualityCategory)quality)), 2,
                                MidpointRounding.AwayFromZero),
                            Weight = StatDefOf.Mass.Worker.GetValue(StatRequest.For(thing, stuff,
                                (QualityCategory)quality))
                        };
                }
        }

        private static IEnumerable<ColonyTradable> ComputeItemsWithQuality(ThingDef thing)
        {
            for (var quality = 2; quality < 7; quality++)
                yield return new ColonyTradable
                {
                    ThingDef = thing.defName,
                    Minified = thing.Minifiable,
                    Quality = quality,
                    BaseValue = (float)Math.Round(StatDefOf.MarketValue.Worker
                            .GetValue(StatRequest.For(thing, null, (QualityCategory)quality)), 2,
                        MidpointRounding.AwayFromZero),
                    Weight = StatDefOf.Mass.Worker.GetValue(StatRequest.For(thing, null, (QualityCategory)quality))
                };
        }

        private static IEnumerable<ColonyTradable> ComputeThingDef(ThingDef thing,
            IReadOnlyDictionary<StuffCategoryDef, List<ThingDef>> stuffData)
        {
            var result = new List<ColonyTradable>();

            if (thing.MadeFromStuff)
                // Item can be made from stuff, Does it have quality too?
                result.AddRange(thing.HasComp(typeof(CompQuality))
                    ? ComputeItemsMadeFromStuffWithQuality(thing, stuffData) // Yes
                    : ComputeItemsMadeFromStuff(thing, stuffData)); // No
            else if (thing.HasComp(typeof(CompQuality)))
                // Has quality but no stuff
                result.AddRange(ComputeItemsWithQuality(thing));
            else
                // Normal item, no quality or Stuff Type
                result.Add(
                    new ColonyTradable
                    {
                        ThingDef = thing.defName,
                        Minified = thing.Minifiable,
                        BaseValue = (float)Math.Round(
                            StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thing, null)), 2,
                            MidpointRounding.AwayFromZero),
                        Weight = StatDefOf.Mass.Worker.GetValue(StatRequest.For(thing, null))
                    }
                );

            return result;
        }

        internal static List<Thing> AllColonyThingsForTrade(Map map)
        {
            return new List<Thing>(GetItemsNearBeacons(map));
        }

        internal static IEnumerable<Thing> GetItemsNearBeacons(Map map, string filterByDefName = null)
        {
            var seenThings = new HashSet<int>();
            foreach (var beacon in Building_OrbitalTradeBeacon.AllPowered(map))
            {
                Logging.WriteDebugMessage($"Found trade beacon @ {beacon.Position.ToString()}");
                foreach (var beaconCell in beacon.TradeableCells)
                {
                    var thingList = beaconCell.GetThingList(map);
                    foreach (var t in thingList)
                    {
                        if (seenThings.Contains(t.thingIDNumber)) continue;
                        // Unpack minified boxes to find out what's in them.
                        var thing = t.GetInnerIfMinified();
                        if (filterByDefName != null && thing.def.defName != filterByDefName ||
                            !CanThisItemBeSold(t)) continue;
                        seenThings.Add(thing.thingIDNumber);
                        yield return t;
                    }
                }
            }
        }

        internal static bool CanThisItemBeSold(Thing t)
        {
            // Make sure it's an item or building and that it can be minified etc.
            if (t.def.category == ThingCategory.Building && !(t is MinifiedThing))
                return false;

            // If it is minified, unpack it to find out what it really is
            t = t.GetInnerIfMinified();

            if (!CanThisThingDefBeSold(t.def)) return false;

            // Not desiccated
            if (t.IsNotFresh()) return false;

            // Don't allow trading dead man clothing
            if (t is Apparel apparel && apparel.WornByCorpse) return false;

            // Can't sell BioCoded
            var compBiocodable = t.TryGetComp<CompBiocodable>();
            if (compBiocodable is null) return true;
            return !compBiocodable.Biocoded;
        }

        private static bool CanThisThingDefBeSold(ThingDef def)
        {
            if (!def.tradeability.PlayerCanSell()) return false;

            if (def.GetStatValueAbstract(StatDefOf.MarketValue) <= 0f) return false;

            if (def.category != ThingCategory.Item &&
                def.category != ThingCategory.Building
            )
                return false;

            return def.category != ThingCategory.Building || def.Minifiable;
        }

        internal static int CalculateThingHitPoints(Thing thing)
        {
            if (thing.def.useHitPoints) return (int)Math.Floor((float)thing.HitPoints / thing.MaxHitPoints * 100.0f);

            return 100;
        }

        #endregion
    }
}