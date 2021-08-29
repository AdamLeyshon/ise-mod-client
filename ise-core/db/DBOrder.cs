#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, DBOrder.cs 2021-02-20

#endregion

using LiteDB;
using Order;

namespace ise_core.db
{
    public class DBOrder
    {
        [BsonId] public string Id { get; set; }

        /// <summary>
        ///     The Id of the Colony that placed it
        /// </summary>
        public string ColonyId { get; set; }

        /// <summary>
        ///     The last known status of the order from the server.
        /// </summary>
        public OrderStatusEnum Status { get; set; }

        /// <summary>
        ///     The Game tick when this order was placed.
        /// </summary>
        public int PlacedTick { get; set; }

        /// <summary>
        ///     The approx. tick when the order will be ready to deliver.
        /// </summary>
        public int DeliveryTick { get; set; }

        public bool ManifestAvailable { get; set; }
    }
}