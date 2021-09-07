#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, isegamecomponent.cs 2021-02-10

#endregion

using System;
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
        private readonly Dictionary<string, Account> _activeAccounts = new Dictionary<string, Account>();

        private readonly Dictionary<string, string> _colonyCache = new Dictionary<string, string>();
        internal string ClientBind;
        private int _nextUpdateTick;
        private bool _spawnToolUsed;
        private bool _firstRunComplete;

        #endregion

        #region Properties

        internal bool SpawnToolUsed
        {
            get => _spawnToolUsed;
            private set
            {
                if (value)
                    // Don't allow unsetting
                    _spawnToolUsed = true;
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
            Logging.WriteDebugMessage("Loaded game");
            base.LoadedGame();
            LoadBinds();
        }

        public override void StartedNewGame()
        {
            Logging.WriteDebugMessage("Start new game");
            base.StartedNewGame();
            LoadBinds();
        }

        private void LoadBinds()
        {
            ClientBind = LoadBind<DBClientBind>(IseCentral.User.UserId);
            if (ClientBind.NullOrEmpty())
            {
                Logging.WriteDebugMessage($"No Client bind for: {IseCentral.User.UserId}");
                return;
            }

            Logging.WriteDebugMessage($"Client bind {ClientBind}");
            // Do other stuff

            if (_firstRunComplete) return;

            StartAccountTracking();
            var currentTick = Current.Game.tickManager.TicksGame;

            // Set the first tick a little ahead of time so we don't
            // overburden the game at startup
            _nextUpdateTick = currentTick + 500;
            _firstRunComplete = true;
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
            if (_colonyCache.TryGetValue(mapId, out var outputId)) return outputId;
            outputId = LoadBind<DBColonyBind>(mapId);
            if (outputId.NullOrEmpty())
                // Logging.WriteDebugMessage($"No colony bind for: {ClientBind}");
                return null;

            _colonyCache.Add(mapId, outputId);
            // Logging.WriteDebugMessage($"Colony bind {outputId}");
            return outputId;
        }

        internal void FlushColonyIdCache()
        {
            _colonyCache.Clear();
        }

        public override void GameComponentTick()
        {
            var currentTick = Current.Game.tickManager.TicksGame;
            base.GameComponentTick();
            if (currentTick < _nextUpdateTick) return;

            if (DateTime.Now - IseCentral.LastHandshakeAttempt > TimeSpan.FromSeconds(30))
            {
                IseCentral.StartHandshakeTask();
                return;
            }

            if (!IseCentral.HandshakeComplete) return;

            Logging.WriteDebugMessage($"Ticking {_activeAccounts.Count} Account Managers");
            foreach (var activeAccountsKey in _activeAccounts.Keys)
                Logging.WriteDebugMessage($"Account Manager: {activeAccountsKey}");

            _nextUpdateTick = currentTick + UpdateTickInterval;

            foreach (var task in _activeAccounts.Values.Select(account => new Task(account.UpdateAsync))) task.Start();
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
                if (!colonyId.NullOrEmpty() && !_activeAccounts.ContainsKey(colonyId))
                    _activeAccounts.Add(colonyId, new Account(colonyId, this));
            }
        }

        internal Account GetAccount(string colonyId)
        {
            if (_activeAccounts.TryGetValue(colonyId, out var account)) return account;
            account = new Account(colonyId, this);
            _activeAccounts.Add(colonyId, account);
            return account;
        }

        #endregion
    }
}