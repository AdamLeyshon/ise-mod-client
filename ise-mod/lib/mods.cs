#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, mods.cs, Created 2021-02-03

#endregion

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ise.lib
{
    internal static class Mods
    {
        internal static IEnumerable<string> GetModList()
        {
            return ModLister.AllInstalledMods
                .Where(x => x.Active)
                .AsEnumerable()
                .Select(mod => mod.PackageId)
                .ToList();
        }
    }
}