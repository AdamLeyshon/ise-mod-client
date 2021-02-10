#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, job_shoponline.cs, Created 2021-02-10

#endregion

using System.Collections.Generic;
using ise.buildings;
using RimWorld;
using Verse;
using Verse.AI;

namespace ise.jobs
{
    public class Job_ISEShopOnline : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) =>
                !((ISEConsole) to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanShopOnlineNow);
            var shopOnline = new Toil();
            shopOnline.initAction = delegate
            {
                var actor = shopOnline.actor;
                if (((ISEConsole) actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanShopOnlineNow)
                {
                    ((ISEConsole) actor.jobs.curJob.targetA).ShopOnline(actor);
                }
            };
            yield return shopOnline;
        }
    }
}