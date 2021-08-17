#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, iseconsole.cs 2021-02-10

#endregion

using System.Collections.Generic;
using System.Linq;
using Common;
using ise.components;
using ise.dialogs;
using ise.jobs;
using ise.lib;
using ise_core.db;
using Order;
using RimWorld;
using Verse;
using Verse.AI;

namespace ise.buildings
{
    public class ISEConsole : Building
    {
        #region Fields

        private CompPowerTrader _powerComp;
        private ISEGameComponent _gc;

        #endregion

        #region Properties

        internal bool CanShopOnlineNow =>
            Spawned &&
            ISEUplink.HasBuilding(Map) &&
            ISEMaterialiser.HasBuilding(Map) &&
            !Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare) &&
            _powerComp.PowerOn;

        internal bool CanWithdrawCashNow => !_gc.ClientBind.NullOrEmpty();

        #endregion

        #region Methods

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _powerComp = GetComp<CompPowerTrader>();
            _gc = Current.Game.GetComponent<ISEGameComponent>();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            var failureReason = GetFailureReason(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
            }
            else
            {
                yield return new FloatMenuOption("ISEShopMenuOption".Translate(),
                    delegate { GiveJobShopOnline(myPawn, this); });

                if (CanWithdrawCashNow)
                    yield return new FloatMenuOption("ISEWithdrawMenuOption".Translate(),
                        delegate { GiveJobWithdrawCash(myPawn, this); });

                var dbOrders = IseCentral.DataCache.GetCollection<DBOrder>(Constants.Tables.Orders);

                // Don't process any more if we don't have a Colony ID yet
                if (!_gc.ClientBindVerified || _gc.GetColonyId(Map).NullOrEmpty()) yield break;
                foreach (var order in _gc.GetAccount(_gc.GetColonyId(Map)).GetActiveOrders
                    .Where(x => x.Status == OrderStatusEnum.Placed))
                {
                    var dbOrder = dbOrders.FindById(order.OrderId);
                    if (dbOrder == null) continue;
                    var deliveryTime = dbOrder.DeliveryTick - Current.Game.tickManager.TicksGame;
                    yield return new FloatMenuOption(
                        $"Order {order.OrderId}, Ready for materialising in {deliveryTime.ToStringTicksToDays()}",
                        null);
                }
            }
        }

        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (!ISEUplink.HasBuilding(Map))
                return new FloatMenuOption("ISENeedUplink".Translate(), null);
            if (!ISEMaterialiser.HasBuilding(Map))
                return new FloatMenuOption("ISENeedMaterialiser".Translate(), null);
            if (!myPawn.CanReach((LocalTargetInfo)this, PathEndMode.InteractionCell, Danger.Some))
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            if (Spawned && Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
            if (!_powerComp.PowerOn)
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            return CanShopOnlineNow ? null : new FloatMenuOption("Cannot use now", null);
        }

        private void GiveJobShopOnline(Pawn myPawn, ISEConsole target)
        {
            myPawn.jobs.TryTakeOrderedJob(new Job(ISEJobDefOf.ISEShopOnlineJob, (LocalTargetInfo)this));
        }

        private void GiveJobWithdrawCash(Pawn myPawn, ISEConsole target)
        {
            myPawn.jobs.TryTakeOrderedJob(new Job(ISEJobDefOf.ISEWithdrawCashJob, (LocalTargetInfo)this));
        }

        internal void ShopOnline(Pawn user)
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            var destination = new DialogMarketDownload(user);
            Find.WindowStack.Add(gc.ClientBindVerified
                ? new DialogBind(user, DialogBind.BindUIType.Colony, destination)
                : new DialogBind(user, DialogBind.BindUIType.Client, destination));
        }

        internal void WithdrawCash(Pawn user)
        {
            var destination = new DialogBankAccountFetch(user, CurrencyEnum.Utc);
            Find.WindowStack.Add(new DialogBind(user, DialogBind.BindUIType.Colony, destination));
        }

        #endregion
    }
}