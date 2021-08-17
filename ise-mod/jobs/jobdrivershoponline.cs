#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, jobdrivershoponline.cs 2021-02-10

#endregion

using System.Collections.Generic;
using ise.buildings;
using Verse.AI;

namespace ise.jobs
{
    // ReSharper disable once UnusedType.Global
    public class JobDriverShopOnline : JobDriver
    {
        #region Methods

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(to =>
                !((ISEConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanShopOnlineNow);
            var shopOnline = new Toil();
            shopOnline.initAction = delegate
            {
                var actor = shopOnline.actor;
                if (((ISEConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanShopOnlineNow)
                    ((ISEConsole)actor.jobs.curJob.targetA).ShopOnline(actor);
            };
            yield return shopOnline;
        }

        #endregion
    }
}