using ise.lib;
using ise_core.db;
using Verse;
using static ise.lib.User;

namespace ise
{
    [StaticConstructorOnStartup]
    public class IseBootStrap
    {
        public static readonly DBUser User;
        public static DBClientBind ClientBind;

        static IseBootStrap()
        {
            User = LoadUserData();

        }
    }
}