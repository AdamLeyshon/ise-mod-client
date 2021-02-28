#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, iseconsole.cs, Created 2021-02-10

#endregion

using System.Collections.Generic;
using ise.components;
using ise.dialogs;
using ise.jobs;
using RimWorld;
using Verse;
using Verse.AI;

namespace ise.buildings
{
    public class ISEConsole : Building
    {
        #region Fields

        private CompPowerTrader powerComp;

        #endregion

        #region Properties

        internal bool CanShopOnlineNow =>
            Spawned &&
            ISEUplink.HasUplink(Map) &&
            !Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) &&
            powerComp.PowerOn;

        #endregion

        #region Methods

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            var failureReason = GetFailureReason(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
                yield break;
            }

            yield return new FloatMenuOption("ISEShopOnline".Translate(),
                delegate { GiveJobShopOnline(myPawn, this); });
        }

        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (!ISEUplink.HasUplink(Map))
                return new FloatMenuOption("ISENeedUplink".Translate(), null);
            if (!myPawn.CanReach((LocalTargetInfo) this, PathEndMode.InteractionCell, Danger.Some))
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            if (Spawned && Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
            if (!powerComp.PowerOn)
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            if (CanShopOnlineNow)
                return null;
            Log.Error($"{myPawn} could not use console for unknown reason.");
            return new FloatMenuOption("Cannot use now", null);
        }

        private void GiveJobShopOnline(Pawn myPawn, ISEConsole target)
        {
            myPawn.jobs.TryTakeOrderedJob(new Job(ISEJobDefOf.ISEShopOnline, (LocalTargetInfo) this));
        }

        internal void ShopOnline(Pawn user)
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            Find.WindowStack.Add(gc.ClientBindVerified
                ? new DialogBind(user, DialogBind.BindUIType.Colony)
                : new DialogBind(user, DialogBind.BindUIType.Client));
        }

        #endregion
    }
}