#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, DBColonyBind.cs, Created 2021-02-10

#endregion

namespace ise_core.db
{
    public class DBColonyBind : IBaseBind
    {
        public string BindId { get; set; }
        public string ParentId { get; set; }
    }
}