#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, consts.cs 2021-02-09

#endregion

using Verse;

namespace ise.lib
{
    internal static class Constants
    {
        #region Nested type: Tables

        internal static class Tables
        {
            #region Fields

            internal const string ColonyBasket = "colony_basket";
            internal const string MarketBasket = "market_basket";
            internal const string Orders = "orders";
            internal const string OrderItems = "order_items";
            internal const string MarketCache = "market_cache";
            internal const string ColonyCache = "colony_cache";
            internal const string Promises = "promise";
            internal const string Users = "user_data";
            internal const string Bindings = "bindings";
            internal const string Delivered = "delivered_items";

            #endregion
        }

        #endregion

        #region Fields

        internal const string ThingDefSilver = "Silver";

        /// <summary>
        ///     This is how often we ask the server to update orders.
        ///     7500 ticks is about every 3 minutes, or 3 in-game hours.
        /// </summary>
        internal const int OrderUpdateTickRate = 7500;


        internal static readonly string DBLocation =
            $"Filename='{GenFilePaths.ConfigFolderPath}\\ISEData.db';Connection=direct";

        #endregion
    }
}