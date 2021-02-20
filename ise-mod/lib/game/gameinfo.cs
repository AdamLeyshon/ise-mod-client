#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, gameinfo.cs, Created 2021-02-09

#endregion

using System.Linq;
using Colony;
using RimWorld;
using UnityEngine;
using Verse;

namespace ise.lib.game
{
    internal static class GameInfo
    {
        #region Methods

        internal static PlatformEnum GetGamePlatform()
        {
            var platformString = Application.platform.ToString();
            PlatformEnum platform;
            if (platformString.Contains("Windows"))
                platform = PlatformEnum.Windows;
            else if (platformString.Contains("Linux"))
                platform = PlatformEnum.Linux;
            else if (platformString.Contains("OSX"))
                platform = PlatformEnum.Mac;
            else
                platform = PlatformEnum.Unknown;

            return platform;
        }

        internal static Faction GetColonyFaction()
        {
            return Find.FactionManager.AllFactions.FirstOrDefault(f => f.IsPlayer);
        }

        internal static string GetCurrentMapName(Map m)
        {
            return m.Parent.Label;
            // foreach (var fb in Find.WorldObjects.SettlementBases.Where(x => x.Faction == Faction.OfPlayer))
            //     if (fb.HasMap && fb.Map == m)
            //         return fb.Label;
            // return "A Colony";
        }

        internal static int GetFactionFirstTile()
        {
            return (from m in Find.Maps where m.ParentFaction.IsPlayer select m.Tile).FirstOrDefault();
        }

        internal static bool HasBaseNameBeenSet(Map m)
        {
            return !m.Parent.Label.NullOrEmpty() && m.Parent.Faction.HasName;
        }

        internal static bool MapIsSettlementOfPlayer(Map m)
        {
            return Find.WorldObjects.SettlementBases.Where(
                    x => x.Faction == Faction.OfPlayer
                )
                .Any(fb => fb.HasMap && fb.Map == m);
        }

        internal static string GetUniqueMapID(Map m)
        {
            return Crypto.GetShaHash($"{m.Tile}{Current.Game.World.info.seedString}");
        }

        #endregion
    }
}