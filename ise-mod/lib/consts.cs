#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, consts.cs, Created 2021-02-09

#endregion

using Verse;

namespace ise.lib
{
    internal static class Constants
    {
        #region Fields

        internal const string ColonyBasketTable = "colony_basket";
        internal const string MarketBasketTable = "market_basket";
        internal const string OrderTable = "orders";
        internal const string MarketCacheTable = "market_cache";
        internal const string ColonyCacheTable = "colony_cache";
        internal const string PromiseTable = "promise";
        internal const string UserTable = "user_data";
        internal const string BindingsTable = "bindings";
        internal const string ThingDefSilver = "Silver";

        internal static readonly string DBLocation =
            $"Filename='{GenFilePaths.ConfigFolderPath}\\ISEData.db';Connection=shared";

        #endregion
    }
}