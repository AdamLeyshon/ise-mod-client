#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, enums.cs, Created 2021-03-07

#endregion

using System;

namespace ise_core.extend
{
    public static class EnumHelpers
    {
        public static T Next<T>(this T src) where T : Enum
        {
            var arr = (T[]) Enum.GetValues(src.GetType());
            var nextItem = Array.IndexOf(arr, src) + 1;
            return (arr.Length == nextItem) ? arr[0] : arr[nextItem];
        }
    }
}