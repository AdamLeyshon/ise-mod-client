#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, iseuplink.cs 2021-02-10

#endregion

using RimWorld;
using Verse;

namespace ise.buildings
{
    public class ISEUplink : Building
    {
        #region Methods

        internal static bool HasBuilding(Map map)
        {
            // If we've got multiple buildings, only one needs to be powered to be true
            var buildingList = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("ISEUplink"));
            foreach (var uplinkThing in buildingList)
            {
                var powerTrader = uplinkThing.TryGetComp<CompPowerTrader>();
                // Break early if we find one that's on
                if (powerTrader != null && powerTrader.PowerOn) return true;
            }

            return false;
        }

        #endregion
    }
}