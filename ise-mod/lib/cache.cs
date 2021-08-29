#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, cache.cs 2021-08-17

#endregion

using System;
using ise_core.db;
using LiteDB;
using static ise.lib.Constants;

namespace ise.lib
{
    internal static class Cache
    {
        #region Nested type: CacheType

        internal enum CacheType
        {
            MarketCache = 0,
            ColonyCache = 1,
            MarketBasket = 2,
            ColonyBasket = 3
        }

        #endregion

        #region Methods

        internal static ILiteCollection<DBCachedTradable> GetCache(
            string colonyId,
            CacheType cacheType)
        {
            string tableName;
            // Replace hyphens in UUID with underscore
            colonyId = colonyId.Replace("-", "_");
            switch (cacheType)
            {
                case CacheType.MarketCache:
                    tableName = Tables.MarketCache;
                    break;
                case CacheType.ColonyCache:
                    tableName = Tables.ColonyCache;
                    break;
                case CacheType.MarketBasket:
                    tableName = Tables.MarketBasket;
                    break;
                case CacheType.ColonyBasket:
                    tableName = Tables.ColonyBasket;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
            }

            // Get the collection (or create, if doesn't exist)
            return IseCentral.DataCache.GetCollection<DBCachedTradable>($"{tableName}_{colonyId}");
        }

        internal static void DropCache(string colonyId, CacheType cacheType)
        {
            string tableName;
            colonyId = colonyId.Replace("-", "_");
            switch (cacheType)
            {
                case CacheType.MarketCache:
                    tableName = Tables.MarketCache;
                    break;
                case CacheType.ColonyCache:
                    tableName = Tables.ColonyCache;
                    break;
                case CacheType.MarketBasket:
                    tableName = Tables.MarketBasket;
                    break;
                case CacheType.ColonyBasket:
                    tableName = Tables.ColonyBasket;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
            }

            IseCentral.DataCache.DropCollection($"{tableName}_{colonyId}");
        }

        #endregion
    }
}