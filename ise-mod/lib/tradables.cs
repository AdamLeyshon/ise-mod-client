#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, tradables.cs, Created 2021-02-03

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
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
                        BaseValue = thing.BaseMarketValue,
                        Weight = thing.BaseMass
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
                            BaseValue = thing.BaseMarketValue,
                            Weight = thing.BaseMass
                        };
                }
        }

        private static IEnumerable<ColonyTradable> ComputeItemsWithQuality(ThingDef thing)
        {
            for (var quality = 2; quality < 7; quality++)
            {
                var qualityObject = (QualityCategory) quality;
                yield return new ColonyTradable
                {
                    ThingDef = thing.defName,
                    Minified = thing.Minifiable,
                    Quality = quality,
                    BaseValue = thing.BaseMarketValue,
                    Weight = thing.BaseMass
                };
            }
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
                        BaseValue = thing.BaseMarketValue,
                        Weight = thing.BaseMass
                    }
                );

            return result;
        }

        #endregion
    }
}