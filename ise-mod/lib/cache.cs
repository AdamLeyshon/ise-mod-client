#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, cache.cs, Created 2021-02-20

#endregion

using System;
using ise_core.db;
using LiteDB;
using static ise.lib.Constants;

namespace ise.lib
{
    public static class Cache
    {
        #region Methods

        internal static ILiteCollection<DBCachedTradable> GetCache(
            ILiteDatabase db,
            string colonyId,
            CacheType cacheType)
        {
            string tableName;
            switch (cacheType)
            {
                case CacheType.MarketCache:
                    tableName = MarketCacheTable;
                    break;
                case CacheType.ColonyCache:
                    tableName = ColonyCacheTable;
                    break;
                case CacheType.MarketBasket:
                    tableName = MarketBasketTable;
                    break;
                case CacheType.ColonyBasket:
                    tableName = ColonyBasketTable;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
            }

            // Get the collection (or create, if doesn't exist)
            return db.GetCollection<DBCachedTradable>($"{tableName}_{colonyId}");
        }

        internal static void DropCache(ILiteDatabase db, string colonyId, CacheType cacheType)
        {
            string tableName;
            switch (cacheType)
            {
                case CacheType.MarketCache:
                    tableName = MarketCacheTable;
                    break;
                case CacheType.ColonyCache:
                    tableName = ColonyCacheTable;
                    break;
                case CacheType.MarketBasket:
                    tableName = MarketBasketTable;
                    break;
                case CacheType.ColonyBasket:
                    tableName = ColonyBasketTable;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
            }

            db.DropCollection($"{tableName}_{colonyId}");
        }

        #endregion

        #region Nested type: CacheType

        internal enum CacheType
        {
            MarketCache = 0,
            ColonyCache = 1,
            MarketBasket = 2,
            ColonyBasket = 3
        }

        #endregion
    }
}