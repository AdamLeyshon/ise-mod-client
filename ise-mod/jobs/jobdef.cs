#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, jobdef.cs 2021-02-10

#endregion

using RimWorld;
using Verse;

namespace ise.jobs
{
    // These are loaded by RW via XML so they appear technically unused in the code.

    [DefOf]
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    public class ISEJobDefOf
    {
        #region Fields

        // ReSharper disable once UnassignedField.Global
        // ReSharper disable once InconsistentNaming
        public static JobDef ISEShopOnlineJob;
        public static JobDef ISEWithdrawCashJob;

        #endregion
    }
}