#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, DBInventory.cs, Created 2021-02-12
#endregion

namespace ise_core.db
{
    public class DBInventory
    {
        // Server Promise
        public string InventoryPromiseId { get; set; }

        // UTC Epoch timestamp of when this offer expires
        public long InventoryPromiseExpires { get; set; }

        // Fixed amount per KG of item to be sold
        public int CollectionChargePerKG { get; set; }

        // Fixed amount per KG of item to be bought
        public int DeliveryChargePerKG { get; set; }

        // Amount of cash in their account already.
        public long AccountBalance { get; set; }
    }
}