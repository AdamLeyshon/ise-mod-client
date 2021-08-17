#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, entrypoint.cs 2020-09-02

#endregion

using ise_core.db;
using LiteDB;
using Verse;
using static ise.lib.User;
using static ise.lib.Constants;

namespace ise
{
    [StaticConstructorOnStartup]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IseCentral
    {
        #region Fields

        internal static readonly DBUser User;

        #endregion

        #region ctor

        static IseCentral()
        {
            DataCache = new LiteDatabase(DBLocation);
            User = LoadUserData();
        }

        #endregion

        #region Properties

        internal static LiteDatabase DataCache { get; }

        #endregion
    }
}