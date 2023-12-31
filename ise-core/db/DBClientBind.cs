#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, DBClientBind.cs 2021-02-09

#endregion

namespace ise_core.db
{
    public class DBClientBind : IBaseBind
    {
        #region IBaseBind Interface Implementations

        public string BindId { get; set; }
        public string ParentId { get; set; }

        #endregion
    }
}