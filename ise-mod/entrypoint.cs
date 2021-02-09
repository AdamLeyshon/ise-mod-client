using ise.lib;
using Verse;
using static ise.lib.User;

namespace ise
{
    [StaticConstructorOnStartup]
    public class IseBootStrap
    {
        static IseBootStrap()
        {
            var userData = LoadUserData();
            var bindId = LoadClientBind(userData.UserId);
            if (bindId.NullOrEmpty())
            {
                Logging.WriteMessage($"No client bind for: {userData.UserId}");
            }
        }
    }
}