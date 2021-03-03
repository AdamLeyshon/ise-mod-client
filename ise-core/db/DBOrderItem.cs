#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, DBOrderItem.cs, Created 2021-02-20

#endregion

namespace ise_core.db
{
    public class DBOrderItem : BaseItem
    {
        #region Properties

        public string OrderId { get; set; }

        /// <summary>
        ///     We hide the base implementation to ensure the ItemCode
        ///     is not used as the Unique ID. Let LiteDB create one itself.
        /// </summary>
        public new string ItemCode { get; set; }

        #endregion
    }
}