#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, BaseItem.cs 2021-02-26
// #endregion

#endregion

using LiteDB;

namespace ise_core.db
{
    public abstract class BaseItem
    {
        /// <summary>
        ///     Unique Item Code, unique per thing, quality and stuff combination.
        /// </summary>
        [BsonId]
        public string ItemCode { get; set; }

        public string ThingDef { get; set; }

        /// <summary>
        ///     If made of stuff, This is the ThingDef
        /// </summary>
        public string Stuff { get; set; }

        public int Quality { get; set; }

        public int Quantity { get; set; }
    }
}