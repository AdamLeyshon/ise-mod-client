#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, DBCachedTradable.cs 2021-02-12

#endregion

using LiteDB;

namespace ise_core.db
{
    public class DBCachedTradable : BaseItem
    {
        #region Properties

        /// <summary>
        ///     How many we want to trade
        /// </summary>
        public int TradedQuantity { get; set; }

        public bool Minified { get; set; }
        public float BaseValue { get; set; }

        /// <summary>
        ///     The price the colony will get for selling
        /// </summary>
        public float WeBuyAt { get; set; }

        /// <summary>
        ///     The price the colony will for for buying
        /// </summary>
        public float WeSellAt { get; set; }

        public float Weight { get; set; }

        // Percentage of HP remaining, floored to int
        public int HitPoints { get; set; }

        public string TranslatedName { get; set; }
        public string IndexedName { get; set; }
        public string TranslatedStuff { get; set; }
        public string Category { get; set; }

        // This is used by Unity to store the text
        // While they edit the Quantity box

        [BsonIgnore] public string UnityTextBuffer { get; set; }

        #endregion
    }
}