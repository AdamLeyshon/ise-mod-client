#region license
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, DebugWindowFix.cs 2022-01-25
#endregion

using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace ise.patches
{
    [HarmonyPatch(typeof(GenTypes), nameof(GenTypes.AllTypes), MethodType.Getter)]
    public class DebugWindowFix
    {
        static IEnumerable<System.Type> Postfix(IEnumerable<System.Type> values)
        {
            foreach (var value in values)
                if(!(value.FullName != null && value.FullName.StartsWith("Google.Protobuf"))) yield return value;
        }
    }
}