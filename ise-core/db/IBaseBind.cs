#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, bind.cs, Created 2021-02-10
#endregion

namespace ise_core.db
{
    public interface IBaseBind
    {
        string BindId { get; set; }   
        string UserId { get; set; }
    }
}