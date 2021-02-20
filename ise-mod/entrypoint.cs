#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, entrypoint.cs, Created 2020-09-02

#endregion

using ise_core.db;
using Verse;
using static ise.lib.User;

namespace ise
{
    [StaticConstructorOnStartup]
    public class IseBootStrap
    {
        #region Fields

        public static readonly DBUser User;
        public static DBClientBind ClientBind;

        #endregion

        #region ctor

        static IseBootStrap()
        {
            User = LoadUserData();
        }

        #endregion
    }
}