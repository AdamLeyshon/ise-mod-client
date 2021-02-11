#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, isegamecomponent.cs, Created 2021-02-10

#endregion

using System;
using ise;
using ise.lib;
using ise_core.db;
using static ise.lib.User;
using Verse;
using static ise.lib.game.GameInfo;

namespace ise.components
{
    public class ISEGameComponent : GameComponent
    {
        internal string ColonyBind;
        internal string ClientBind;
        private bool spawnToolUsed = false;

        public ISEGameComponent(Game game)
        {
            Logging.WriteMessage($"Game Component installed");
        }

        public bool SpawnToolUsed
        {
            get => spawnToolUsed;
            private set
            {
                if (value)
                {
                    // Don't allow unsetting
                    spawnToolUsed = true;
                }
            }
        }

        public override void LoadedGame()
        {
            Logging.WriteMessage($"Loaded game");
            base.LoadedGame();
            LoadBinds();
        }

        public override void StartedNewGame()
        {
            Logging.WriteMessage($"Start new game");
            base.StartedNewGame();
            LoadBinds();
        }

        private void LoadBinds()
        {
            ClientBind = LoadBind<DBClientBind>(IseBootStrap.User.UserId);
            if (ClientBind.NullOrEmpty())
            {
                Logging.WriteMessage($"No Client bind for: {IseBootStrap.User.UserId}");
                return;
            }
            
            Logging.WriteMessage($"Client bind {ClientBind}");

            ColonyBind = LoadBind<DBColonyBind>(ClientBind);
            if (ColonyBind.NullOrEmpty())
            {
                Logging.WriteMessage($"No colony bind for: {ClientBind}");
                return;
            }

            Logging.WriteMessage($"Colony bind {ColonyBind}");

            // Do other stuff
        }

        internal bool IsValidLocationForIse(Map m)
        {
            // Can only use ISE where you own the map and it's a settlement with a name
            var mapID = GetUniqueMapID(m);
            Logging.WriteMessage($"Map ID of caller is {mapID}");
            return GetColonyFaction().HasName && m.Parent.HasName && MapIsSettlementOfPlayer(m);
        }

        /// <summary>
        /// Set if the bind has already been verified since game start.
        /// </summary>
        public bool BindVerified { get; set; }
    }
}