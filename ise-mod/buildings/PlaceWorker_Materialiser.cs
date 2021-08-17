#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, PlaceWorker_Materialiser.cs 2021-08-13

#endregion

using RimWorld;
using Verse;

namespace ise.buildings
{
    public class PlaceWorker_Materialiser : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(
            BuildableDef checkingDef,
            IntVec3 loc,
            Rot4 rot,
            Map map,
            Thing thingToIgnore = null,
            Thing thing = null)
        {
            if (ISEMaterialiser.HasBuilding(map)) return "ISEMaterialiserCanOnlyBuildOne".Translate();
            return true;
        }

        public override bool ForceAllowPlaceOver(BuildableDef otherDef)
        {
            return otherDef == ThingDefOf.SteamGeyser;
        }
    }
}