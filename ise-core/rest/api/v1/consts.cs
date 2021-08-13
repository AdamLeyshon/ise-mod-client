#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, consts.cs 2021-07-09
// #endregion

#endregion

namespace ise_core.rest.api.v1
{
    public static class Constants
    {
        #region Fields

        public const string URLPrefix = "/api/v1/";
#if NETLOCAL
        public const string Server = "https://ise-local.thecodecache.net";
#elif DEBUG
        public const string Server = "https://ise-dev.thecodecache.net";
#else
        public const string Server = "https://ise-prod.thecodecache.net";
#endif

        #endregion
    }
}