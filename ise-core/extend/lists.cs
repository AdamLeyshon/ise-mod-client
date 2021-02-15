#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, lists.cs, Created 2021-02-06
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace ise_core.extend
{
    public static class IEnumHelpers
    {
        #region Methods

        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            // note: creating a Random instance each call may not be correct for you,
            // consider a thread-safe static instance
            var r = new Random();  
            var list = enumerable as IList<T> ?? enumerable.ToList(); 
            return list.Count == 0 ? default(T) : list[r.Next(0, list.Count)];
        }

        #endregion
    }
}