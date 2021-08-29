#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, DBInventory.cs 2021-02-12

#endregion

using LiteDB;

namespace ise_core.db
{
    public class DBInventoryPromise
    {
        #region Properties

        /// <summary>
        ///     Server Promise Id
        /// </summary>
        public string InventoryPromiseId { get; set; }

        /// <summary>
        ///     The Colony this promise belongs to
        /// </summary>
        [BsonId]
        public string ColonyId { get; set; }

        /// <summary>
        ///     UTC Epoch timestamp of when this offer expires
        /// </summary>
        public long InventoryPromiseExpires { get; set; }

        /// <summary>
        ///     Fixed amount per KG of item to be sold
        /// </summary>
        public int CollectionChargePerKG { get; set; }

        /// <summary>
        ///     Fixed amount per KG of item to be bought
        /// </summary>
        public int DeliveryChargePerKG { get; set; }

        /// <summary>
        ///     Amount of cash in their account already.
        /// </summary>
        public long AccountBalance { get; set; }

        #endregion
    }
}