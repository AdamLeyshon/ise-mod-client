#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, IBaseBind.cs 2021-02-10

#endregion

namespace ise_core.db
{
    public interface IBaseBind
    {
        #region Properties

        string BindId { get; set; }
        string ParentId { get; set; }

        #endregion
    }
}