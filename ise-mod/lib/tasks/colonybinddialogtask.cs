#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, client_bind.cs, Created 2021-02-11

#endregion

using System;
using System.Threading.Tasks;
using Colony;
using ise.dialogs;
using RestSharp;
using Verse;
using static ise_core.rest.Helpers;
using static ise.lib.User;
using static ise_core.rest.api.v1.consts;
using ise.components;
using ise_core.db;
using RimWorld;
using GameInfo = ise.lib.game.GameInfo;

namespace ise.lib.tasks
{
    internal class ColonyBindDialogTask : AbstractDialogTask
    {
        private enum State
        {
            Start,
            Create,
            GetDetails,
            UpdateData,
            UpdateMods,
            UpdateTradables,
            Done,
            Error,
        }

        private State state;
        private Task task;
        private readonly ISEGameComponent gc;

        public ColonyBindDialogTask(IDialog dialog) : base(dialog)
        {
            state = State.Start;
            gc = Current.Game.GetComponent<ISEGameComponent>();
        }

        public override void Update()
        {
            // Handle task errors first.
            if (task != null && task.IsFaulted)
            {
                LogTaskError();
            }

            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartCreate();
                    break;
                case State.Create:
                    Dialog.DialogMessage = "Registering Colony";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessColonyCreateReply(((Task<ColonyData>) task).Result);
                    }

                    break;
                case State.GetDetails:
                    Dialog.DialogMessage = "Checking Details";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessGetColonyDataReply(((Task<ColonyData>) task).Result);
                    }

                    break;
                case State.UpdateData:
                    Dialog.DialogMessage = "Updating Colony Details";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessUpdateColonyReply(((Task<ColonyData>) task).Result);
                    }

                    break;
                case State.UpdateMods:
                    Dialog.DialogMessage = "Updating Colony Mods";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessColonyModsUpdateReply(((Task<ColonyModsSetReply>) task).Result);
                    }

                    break;
                case State.UpdateTradables:
                    Dialog.DialogMessage = "Updating Colony Tradables";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessColonyTradablesReply(((Task<ColonyTradableSetReply>) task).Result);
                    }

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
            state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {task.Exception}");
            task = null;
        }

        private void StartCreate()
        {
            if (!gc.ColonyBind.NullOrEmpty())
            {
                // Jump straight to verify
                StartColonyGet();
            }
            else
            {
                var map = Dialog.pawn.Map;
                var colonyData = new ColonyData
                {
                    Name = GameInfo.GetCurrentMapName(map),
                    FactionName = GameInfo.GetColonyFaction().Name,
                    MapId = map.Parent.ID,
                    Tick = Current.Game.tickManager.TicksGame,
                    UsedDevMode = gc.SpawnToolUsed,
                    GameVersion = VersionControl.CurrentVersionStringWithRev,
                    Platform = GameInfo.GetGamePlatform(),
                    Seed = Current.Game.World.info.Seed.ToString(),
                    Location = map.info.Tile.ToString()
                };

                var request = new ColonyCreateRequest {ClientBindId = gc.ClientBind, Data = colonyData};

                task = SendAndParseReplyAsync(
                    request,
                    ColonyData.Parser,
                    $"{URLPrefix}colony/",
                    Method.POST,
                    gc.ClientBind
                );
                task.Start();
                state = State.Create;
            }
        }

        private void ProcessColonyCreateReply(ColonyData reply)
        {
            Logging.WriteMessage($"Got new Colony ID: {reply.ColonyId}");

            SaveBind<DBColonyBind>(gc.ClientBind, reply.ColonyId);
            gc.ColonyBind = reply.ColonyId;

            // Jump to Colony Mods
            StartColonyUpdateMods();
        }

        private void StartColonyGet()
        {
            state = State.GetDetails;
            var request = new ColonyGetRequest
            {
                ClientBindId = gc.ClientBind,
                ColonyId = gc.ColonyBind,
            };
            task = SendAndParseReplyAsync(
                request,
                ColonyData.Parser,
                $"{URLPrefix}colony/get",
                Method.POST,
                gc.ClientBind
            );
            task.Start();
        }

        private void ProcessGetColonyDataReply(ColonyData reply)
        {
            Logging.WriteMessage($"Colony ID confirmed: {reply.ColonyId}");

            // Now update the colony data
            StartColonyUpdate();
        }

        private void StartColonyUpdate()
        {
            Logging.WriteMessage($"Updating Colony Details for {gc.ColonyBind}");
            var request = new ColonyUpdateRequest
            {
                ClientBindId = gc.ClientBind,
                Data = new ColonyData
                {
                    ColonyId = gc.ColonyBind,
                    Tick = Current.Game.tickManager.TicksGame,
                    UsedDevMode = gc.SpawnToolUsed,
                    GameVersion = VersionControl.CurrentVersionStringWithRev,
                }
            };

            task = SendAndParseReplyAsync(
                request,
                ColonyData.Parser,
                $"{URLPrefix}colony/",
                Method.PATCH,
                gc.ClientBind
            );
            task.Start();
            state = State.UpdateData;
        }

        private void ProcessUpdateColonyReply(ColonyData reply)
        {
            Logging.WriteMessage($"Server accepted colony update");
            StartColonyUpdateMods();
        }

        private void StartColonyUpdateMods()
        {
            Logging.WriteMessage($"Update Colony mods {gc.ColonyBind}");
            var request = new ColonyModsSetRequest()
            {
                ClientBindId = gc.ClientBind,
                ColonyId = gc.ColonyBind,
            };
            request.ModName.AddRange(Mods.GetModList());

            task = SendAndParseReplyAsync(
                request,
                ColonyModsSetReply.Parser,
                $"{URLPrefix}colony/mods",
                Method.POST,
                gc.ClientBind
            );
            task.Start();
            state = State.UpdateMods;
        }

        private void ProcessColonyModsUpdateReply(ColonyModsSetReply reply)
        {
            Logging.WriteMessage($"Server accepted colony mods");
            StartColonyUpdateTradables();
        }

        private void StartColonyUpdateTradables()
        {
            Logging.WriteMessage($"Update Colony tradables {gc.ColonyBind}");

            task = new Task<ColonyTradableSetReply>(delegate
            {
                var request = new ColonyTradableSetRequest()
                {
                    ClientBindId = gc.ClientBind,
                    ColonyId = gc.ColonyBind,
                };
                request.Item.AddRange(Tradables.GetAllTradables());

                return SendAndParseReply(
                    request,
                    ColonyTradableSetReply.Parser,
                    $"{URLPrefix}colony/tradables",
                    Method.POST,
                    gc.ClientBind
                );
            });

            task.Start();
            state = State.UpdateTradables;
        }

        private void ProcessColonyTradablesReply(ColonyTradableSetReply reply)
        {
            Logging.WriteMessage($"Server accepted colony tradables");
            state = State.Done;
        }
    }
}