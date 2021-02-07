#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, system.cs, Created 2021-02-03

#endregion

using static ise.lib.Logging;

namespace ise.lib.API.v1
{
    public static class System
    {
        public static void Hello()
        {
            LogWriter.WriteMessage($"API server version: {ise_core.rest.api.v1.System.Hello()}");
        }
    }
}