#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, colonybinddialogtask.cs 2021-02-11

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Colony;
using ise.components;
using ise.dialogs;
using ise_core.db;
using RestSharp;
using RimWorld;
using Verse;
using static ise_core.rest.Helpers;
using static ise.lib.User;
using static ise_core.rest.api.v1.Constants;
using static ise_core.extend.MoreEnumerable;
using GameInfo = ise.lib.game.GameInfo;

namespace ise.lib.tasks
{
    internal class ColonyBindDialogTask : AbstractDialogTask
    {
        #region ctor

        public ColonyBindDialogTask(IDialog dialog, Pawn userPawn) : base(dialog)
        {
            _state = State.Start;
            _gc = Current.Game.GetComponent<ISEGameComponent>();
            _pawn = userPawn;
            _colonyId = _gc.GetColonyId(_pawn.Map);
        }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Create,
            GetDetails,
            UpdateData,
            UpdateMods,
            UpdateTradables,
            Done,
            Error
        }

        #endregion

        #region Fields

        private readonly ISEGameComponent _gc;
        private readonly Pawn _pawn;
        private string _colonyId;

        private State _state;
        private Task _task;

        #endregion

        #region Methods

        public override void Update()
        {
            // Handle task errors first.
            if (_task != null && _task.IsFaulted)
            {
                if (_state == State.GetDetails)
                {
                    // Handle a 404 Colony not found:
                    if (_task.Exception?.InnerException != null &&
                        _task.Exception.InnerException.Message.Contains("404"))
                    {
                        Logging.WriteDebugMessage($"Colony ID {_colonyId} doesn't exist, re-registering.");
                        
                        // Delete the bind and re-create
                        DeleteBind<DBColonyBind>(_colonyId);
                        _gc.FlushColonyIdCache();
                        _colonyId = string.Empty;
                        _state = State.Start;
                    }
                    else
                    {
                        LogTaskError();
                    }
                }
                else
                {
                    LogTaskError();
                }
            }

            switch (_state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    if (_task == null) StartCreate();
                    break;
                case State.Create:
                    Dialog.DialogMessage = "Registering Colony";
                    if (_task != null && _task.IsCompleted) ProcessColonyCreateReply(((Task<ColonyData>)_task).Result);

                    break;
                case State.GetDetails:
                    Dialog.DialogMessage = "Checking Details";
                    if (_task != null && _task.IsCompleted) ProcessGetColonyDataReply(((Task<ColonyData>)_task).Result);

                    break;
                case State.UpdateData:
                    Dialog.DialogMessage = "Updating Colony Details";
                    if (_task != null && _task.IsCompleted) ProcessUpdateColonyReply(((Task<ColonyData>)_task).Result);

                    break;
                case State.UpdateMods:
                    Dialog.DialogMessage = "Updating Colony Mods";
                    if (_task != null && _task.IsCompleted)
                        ProcessColonyModsUpdateReply(((Task<ColonyModsSetReply>)_task).Result);

                    break;
                case State.UpdateTradables:
                    Dialog.DialogMessage = "Updating Colony Tradables";
                    if (_task != null && _task.IsCompleted)
                        ProcessColonyTradablesReply(((Task<bool>)_task).Result);

                    break;
                case State.Done:
                    Dialog.DialogMessage = "Colony Data OK";
                    Done = true;
                    break;
                case State.Error:
                    Dialog.CloseDialog();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogTaskError()
        {
            _state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {_task.Exception}");
            _task = null;
        }

        private void StartCreate()
        {
            if (!_colonyId.NullOrEmpty())
            {
                // Jump straight to verify
                StartColonyGet();
            }
            else
            {
                var map = Dialog.Pawn.Map;
                var colonyData = new ColonyData
                {
                    Name = GameInfo.GetCurrentMapName(map),
                    FactionName = GameInfo.GetColonyFaction().Name,
                    MapId = map.Parent.ID,
                    Tick = Current.Game.tickManager.TicksGame,
                    UsedDevMode = _gc.SpawnToolUsed,
                    GameVersion = VersionControl.CurrentVersionStringWithRev,
                    Platform = GameInfo.GetGamePlatform(),
                    Seed = Current.Game.World.info.Seed.ToString(),
                    Location = map.info.Tile.ToString()
                };

                var request = new ColonyCreateRequest { ClientBindId = _gc.ClientBind, Data = colonyData };

                _task = SendAndParseReplyAsync(
                    request,
                    ColonyData.Parser,
                    $"{URLPrefix}colony/",
                    Method.POST,
                    _gc.ClientBind
                );
                _task.Start();
                _state = State.Create;
            }
        }

        private void ProcessColonyCreateReply(ColonyData reply)
        {
            Logging.WriteDebugMessage($"Got new Colony ID: {reply.ColonyId}");

            SaveBind<DBColonyBind>(GameInfo.GetUniqueMapID(_pawn.Map), reply.ColonyId);
            _gc.FlushColonyIdCache();
            _colonyId = reply.ColonyId;

            // Jump to Colony Mods
            StartColonyUpdateMods();
        }

        private void StartColonyGet()
        {
            _state = State.GetDetails;
            var request = new ColonyGetRequest
            {
                ClientBindId = _gc.ClientBind,
                ColonyId = _colonyId
            };
            _task = SendAndParseReplyAsync(
                request,
                ColonyData.Parser,
                $"{URLPrefix}colony/get",
                Method.POST,
                _gc.ClientBind
            );
            _task.Start();
        }

        private void ProcessGetColonyDataReply(ColonyData reply)
        {
            Logging.WriteDebugMessage($"Colony ID confirmed: {reply.ColonyId}");
            // Now update the colony data
            StartColonyUpdate();
        }

        private void StartColonyUpdate()
        {
            Logging.WriteDebugMessage($"Updating Colony Details for {_gc.GetColonyId(_pawn.Map)}");
            var request = new ColonyUpdateRequest
            {
                ClientBindId = _gc.ClientBind,
                Data = new ColonyData
                {
                    ColonyId = _colonyId,
                    Tick = Current.Game.tickManager.TicksGame,
                    UsedDevMode = _gc.SpawnToolUsed,
                    GameVersion = VersionControl.CurrentVersionStringWithRev
                }
            };

            _task = SendAndParseReplyAsync(
                request,
                ColonyData.Parser,
                $"{URLPrefix}colony/",
                Method.PATCH,
                _gc.ClientBind
            );
            _task.Start();
            _state = State.UpdateData;
        }

        private void ProcessUpdateColonyReply(ColonyData reply)
        {
            Logging.WriteDebugMessage("Server accepted colony update");
            StartColonyUpdateMods();
        }

        private void StartColonyUpdateMods()
        {
            Logging.WriteDebugMessage($"UpdateAsync Colony mods {_colonyId}");
            var request = new ColonyModsSetRequest
            {
                ClientBindId = _gc.ClientBind,
                ColonyId = _colonyId
            };
            request.ModName.AddRange(Mods.GetModList());

            _task = SendAndParseReplyAsync(
                request,
                ColonyModsSetReply.Parser,
                $"{URLPrefix}colony/mods",
                Method.POST,
                _gc.ClientBind
            );
            _task.Start();
            _state = State.UpdateMods;
        }

        private void ProcessColonyModsUpdateReply(ColonyModsSetReply reply)
        {
            Logging.WriteDebugMessage("Server accepted colony mods");
            StartColonyUpdateTradables();
        }

        private void StartColonyUpdateTradables()
        {
            Logging.WriteDebugMessage($"UpdateAsync Colony tradables {_colonyId}");

            _task = new Task<bool>(() =>
            {
                var awaitTasks = new List<Task<bool>>();
                var firstBatch = true;
                var response = false;
                var tradables = Tradables.GetAllTradables();
                foreach (var batch in tradables.Batch(25_000))
                {
                    var colonyTradables = batch.ToList();
                    Logging.WriteDebugMessage($"Sending {colonyTradables.Count} tradables ");
                    switch (firstBatch)
                    {
                        case true:
                            response = ise_core.rest.api.v1.Colony.SetTradablesList(
                                _gc.ClientBind,
                                _colonyId,
                                colonyTradables);
                            firstBatch = false;
                            break;
                        case false:
                            if (response)
                            {
                                var batchTask = new Task<bool>(() => ise_core.rest.api.v1.Colony.SetTradablesList(
                                    _gc.ClientBind,
                                    _colonyId,
                                    colonyTradables,
                                    false));
                                batchTask.Start();
                                awaitTasks.Add(batchTask);
                            }
                            else
                            {
                                return false;
                            }

                            break;
                    }
                }

                if (awaitTasks.Count == 0) return response;
                while (awaitTasks.Select(t => t.IsCompleted).Any(s => !s)) Thread.Sleep(10);
                return awaitTasks.All(t => t.Result);
            });

            _task.Start();
            _state = State.UpdateTradables;
        }

        private void ProcessColonyTradablesReply(bool reply)
        {
            if (!reply)
            {
                _state = State.Error;
                Logging.WriteErrorMessage("Server did not accept colony tradables");
            }

            Logging.WriteDebugMessage("Server accepted colony tradables");
            _state = State.Done;
        }

        #endregion
    }
}