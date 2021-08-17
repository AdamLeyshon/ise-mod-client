#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, isegamecomponent.cs 2021-02-10

#endregion

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ise.lib;
using ise.lib.state.managers;
using ise_core.db;
using Verse;
using static ise.lib.User;
using static ise.lib.game.GameInfo;

namespace ise.components
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    public class ISEGameComponent : GameComponent
    {
        #region ctor

        public ISEGameComponent(Game game)
        {
            Logging.WriteDebugMessage("Game Component installed");
        }

        #endregion

        #region Fields

        private const int UpdateTickInterval = 2000;
        private readonly Dictionary<string, Account> activeAccounts = new Dictionary<string, Account>();

        private readonly Dictionary<string, string> colonyCache = new Dictionary<string, string>();
        internal string ClientBind;
        private int nextUpdateTick;
        private bool spawnToolUsed;
        private bool firstRunComplete;

        #endregion

        #region Properties

        internal bool SpawnToolUsed
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
        internal bool ClientBindVerified { get; set; }

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
            ClientBind = LoadBind<DBClientBind>(IseCentral.User.UserId);
            if (ClientBind.NullOrEmpty())
            {
                Logging.WriteMessage($"No Client bind for: {IseCentral.User.UserId}");
                return;
            }

            Logging.WriteMessage($"Client bind {ClientBind}");
            // Do other stuff

            if (firstRunComplete) return;

            StartAccountTracking();
            var currentTick = Current.Game.tickManager.TicksGame;

            // Set the first tick a little ahead of time so we don't
            // overburden the game at startup
            nextUpdateTick = currentTick + 500;
            firstRunComplete = true;
        }

        internal bool IsValidLocationForIse(Map map)
        {
            // Can only use ISE where you own the map and it's a settlement with a name
            var mapID = GetUniqueMapID(map);
            Logging.WriteDebugMessage($"Map ID of caller is {mapID}");
            return GetColonyFaction().HasName && map.Parent.HasName && MapIsSettlementOfPlayer(map);
        }

        internal string GetColonyId(Map map)
        {
            var sf = new StackTrace().GetFrame(1);
            // Logging.WriteDebugMessage(
            //     $"Colony bind lookup from {sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod().Name}");

            var mapId = GetUniqueMapID(map);
            if (colonyCache.TryGetValue(mapId, out var outputId)) return outputId;
            outputId = LoadBind<DBColonyBind>(mapId);
            if (outputId.NullOrEmpty())
                // Logging.WriteDebugMessage($"No colony bind for: {ClientBind}");
                return null;

            colonyCache.Add(mapId, outputId);
            // Logging.WriteDebugMessage($"Colony bind {outputId}");
            return outputId;
        }

        internal void FlushColonyIdCache()
        {
            colonyCache.Clear();
        }

        public override void GameComponentTick()
        {
            var currentTick = Current.Game.tickManager.TicksGame;
            base.GameComponentTick();
            if (currentTick < nextUpdateTick) return;
            Logging.WriteDebugMessage($"Ticking {activeAccounts.Count} Account Managers");
            foreach (var activeAccountsKey in activeAccounts.Keys)
                Logging.WriteDebugMessage($"Account Manager: {activeAccountsKey}");

            nextUpdateTick = currentTick + UpdateTickInterval;

            foreach (var task in activeAccounts.Values.Select(account => new Task(account.UpdateAsync))) task.Start();
        }

        private void StartAccountTracking()
        {
            var player = GetColonyFaction();
            var maps = Current.Game.Maps.Where(map =>
                map.ParentFaction == player &&
                MapIsSettlementOfPlayer(map)
            );
            foreach (var map in maps)
            {
                var colonyId = GetColonyId(map);
                if (!colonyId.NullOrEmpty() && !activeAccounts.ContainsKey(colonyId))
                    activeAccounts.Add(colonyId, new Account(colonyId, this));
            }
        }

        internal Account GetAccount(string colonyId)
        {
            if (activeAccounts.TryGetValue(colonyId, out var account)) return account;
            account = new Account(colonyId, this);
            activeAccounts.Add(colonyId, account);
            return account;
        }

        #endregion
    }
}