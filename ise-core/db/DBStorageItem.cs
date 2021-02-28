#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, DBStorageItem.cs, Created 2021-02-26
#endregion

using LiteDB;

namespace ise_core.db
{
    public class DBStorageItem : BaseItem
    {
        /// <summary>
        /// Delivered items are linked to the colony.
        /// </summary>
        public string ColonyId { get; set; }
        
        // We store this so we don't have to work it out
        // each time the colony needs to update wealth
        // So we calculate it at when written to DB and
        // Multiply by quantity.
        public int Value { get; set; }
    }
}