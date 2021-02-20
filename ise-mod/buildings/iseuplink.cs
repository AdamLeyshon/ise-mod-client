#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, iseuplink.cs, Created 2021-02-10

#endregion

using RimWorld;
using Verse;

namespace ise.buildings
{
    public class ISEUplink : Building
    {
        #region Methods

        internal static bool HasUplink(Map map)
        {
            // If we've got multiple uplinks, only one needs to be powered to have comms
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