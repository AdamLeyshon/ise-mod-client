#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, mods.cs 2021-02-03

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace ise.lib
{
    internal static class Mods
    {
        #region Methods

        internal static IEnumerable<string> GetModList()
        {
            return ModLister.AllInstalledMods
                .Where(x => x.Active)
                .AsEnumerable()
                .Select(mod => mod.PackageId)
                .ToList();
        }

        private static Assembly getISEAssembly()
        {
            return typeof(Mods).Assembly;
        }

        internal static Version getIseVersion()
        {
            return getISEAssembly().GetName().Version;
        }

        #endregion
    }
}