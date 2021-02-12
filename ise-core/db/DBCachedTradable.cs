#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, DBCachedTradable.cs, Created 2021-02-12

#endregion

namespace ise_core.db
{
    public class DBCachedTradable
    {
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
        public string ItemCode { get; set; }
        public int Quality { get; set; }
        public int Quantity { get; set; }
        public bool Minified { get; set; }
        public float BaseValue { get; set; }
        public float WeBuyAt { get; set; }
        public float WeSellAt { get; set; }
        public string Stuff { get; set; }
        public float Weight { get; set; }
    }
}