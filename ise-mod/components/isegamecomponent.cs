#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, isegamecomponent.cs, Created 2021-02-10

#endregion

using System.Collections.Generic;
using ise.lib;
using ise_core.db;
using Verse;
using static ise.lib.User;
using static ise.lib.game.GameInfo;

namespace ise.components
{
    public class ISEGameComponent : GameComponent
    {
        #region Fields

        private readonly Dictionary<string, string> colonyCache = new Dictionary<string, string>();
        internal string ClientBind;
        private bool spawnToolUsed;

        #endregion

        #region ctor

        public ISEGameComponent(Game game)
        {
            Logging.WriteMessage("Game Component installed");
        }

        #endregion

        #region Properties

        public bool SpawnToolUsed
        {
            get => spawnToolUsed;
            private set
            {
                if (value)
                    // Don't allow unsetting
                    spawnToolUsed = true;
            }
        }

        /// <summary>
        ///     Set if the bind has already been verified since game start.
        /// </summary>
        public bool ClientBindVerified { get; set; }

        #endregion

        #region Methods

        public override void LoadedGame()
        {
            Logging.WriteMessage("Loaded game");
            base.LoadedGame();
            LoadBinds();
        }

        public override void StartedNewGame()
        {
            Logging.WriteMessage("Start new game");
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
            // Do other stuff
        }

        internal bool IsValidLocationForIse(Map m)
        {
            // Can only use ISE where you own the map and it's a settlement with a name
            var mapID = GetUniqueMapID(m);
            Logging.WriteMessage($"Map ID of caller is {mapID}");
            return GetColonyFaction().HasName && m.Parent.HasName && MapIsSettlementOfPlayer(m);
        }

        internal string GetColonyId(Map m)
        {
            var mapId = GetUniqueMapID(m);
            if (colonyCache.TryGetValue(mapId, out var outputId)) return outputId;
            outputId = LoadBind<DBColonyBind>(mapId);
            if (outputId.NullOrEmpty())
            {
                Logging.WriteMessage($"No colony bind for: {ClientBind}");
                return null;
            }

            colonyCache.Add(mapId, outputId);
            Logging.WriteMessage($"Colony bind {outputId}");
            return outputId;
        }

        #endregion
    }
}