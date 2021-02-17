#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, DBCachedTradable.cs, Created 2021-02-12

#endregion

using LiteDB;

namespace ise_core.db
{
    public class DBCachedTradable
    {
        #region Properties

        /*
        message Tradable {
        string ThingDef = 1;
        string ItemCode = 5;
        int32 Quality = 10;
        int32 Quantity = 15;
        bool Minified = 20;
        float BaseValue = 25;
        float WeBuyAt = 30;
        float WeSellAt = 35;
        string Stuff = 40;
        float Weight = 45;
        */

        public string ThingDef { get; set; }
        [BsonId] public string ItemCode { get; set; }
        public int Quality { get; set; }

        /// <summary>
        ///  How many we have in total to trade
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// How many we want to trade
        /// </summary>
        public int TradedQuantity { get; set; }

        public bool Minified { get; set; }
        public float BaseValue { get; set; }

        /// <summary>
        /// The price the colony will get for selling
        /// </summary>
        public float WeBuyAt { get; set; }

        /// <summary>
        /// The price the colony will for for buying
        /// </summary>
        public float WeSellAt { get; set; }

        /// <summary>
        /// If made of stuff, This is the ThingDef
        /// </summary>
        public string Stuff { get; set; }

        public float Weight { get; set; }

        // Percentage of HP remaining, floored to int
        public int HitPoints { get; set; }

        public string TranslatedName { get; set; }
        public string TranslatedStuff { get; set; }
        public string Category { get; set; }

        // This is used by Unity to store the text
        // While they edit the Quantity box

        [BsonIgnore] public string UnityTextBuffer { get; set; }

        #endregion
    }
}