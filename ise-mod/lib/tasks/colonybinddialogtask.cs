#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, colonybinddialogtask.cs, Created 2021-02-11

#endregion

using System;
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
using GameInfo = ise.lib.game.GameInfo;

namespace ise.lib.tasks
{
    internal class ColonyBindDialogTask : AbstractDialogTask
    {
        #region Fields

        private readonly ISEGameComponent gc;
        private readonly Pawn pawn;
        private string colonyId;

        private State state;
        private Task task;

        #endregion

        #region ctor

        public ColonyBindDialogTask(IDialog dialog, Pawn userPawn) : base(dialog)
        {
            state = State.Start;
            gc = Current.Game.GetComponent<ISEGameComponent>();
            pawn = userPawn;
            colonyId = gc.GetColonyId(pawn.Map);
        }

        #endregion

        #region Methods

        public override void Update()
        {
            // Handle task errors first.
            if (task != null && task.IsFaulted) LogTaskError();

            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartCreate();
                    break;
                case State.Create:
                    Dialog.DialogMessage = "Registering Colony";
                    if (task != null && task.IsCompleted) ProcessColonyCreateReply(((Task<ColonyData>) task).Result);

                    break;
                case State.GetDetails:
                    Dialog.DialogMessage = "Checking Details";
                    if (task != null && task.IsCompleted) ProcessGetColonyDataReply(((Task<ColonyData>) task).Result);

                    break;
                case State.UpdateData:
                    Dialog.DialogMessage = "Updating Colony Details";
                    if (task != null && task.IsCompleted) ProcessUpdateColonyReply(((Task<ColonyData>) task).Result);

                    break;
                case State.UpdateMods:
                    Dialog.DialogMessage = "Updating Colony Mods";
                    if (task != null && task.IsCompleted)
                        ProcessColonyModsUpdateReply(((Task<ColonyModsSetReply>) task).Result);

                    break;
                case State.UpdateTradables:
                    Dialog.DialogMessage = "Updating Colony Tradables";
                    if (task != null && task.IsCompleted)
                        ProcessColonyTradablesReply(((Task<bool>) task).Result);

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
            if (!colonyId.NullOrEmpty())
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
            SaveBind<DBMapBind>(GameInfo.GetUniqueMapID(pawn.Map), reply.ColonyId);
            colonyId = reply.ColonyId;

            // Jump to Colony Mods
            StartColonyUpdateMods();
        }

        private void StartColonyGet()
        {
            state = State.GetDetails;
            var request = new ColonyGetRequest
            {
                ClientBindId = gc.ClientBind,
                ColonyId = colonyId,
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
            Logging.WriteMessage($"Updating Colony Details for {gc.GetColonyId(pawn.Map)}");
            var request = new ColonyUpdateRequest
            {
                ClientBindId = gc.ClientBind,
                Data = new ColonyData
                {
                    ColonyId = colonyId,
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
            Logging.WriteMessage("Server accepted colony update");
            StartColonyUpdateMods();
        }

        private void StartColonyUpdateMods()
        {
            Logging.WriteMessage($"UpdateAsync Colony mods {colonyId}");
            var request = new ColonyModsSetRequest
            {
                ClientBindId = gc.ClientBind,
                ColonyId = colonyId,
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
            Logging.WriteMessage("Server accepted colony mods");
            StartColonyUpdateTradables();
        }

        private void StartColonyUpdateTradables()
        {
            Logging.WriteMessage($"UpdateAsync Colony tradables {colonyId}");

            task = new Task<bool>(() => ise_core.rest.api.v1.Colony.SetTradablesList(
                gc.ClientBind,
                colonyId,
                Tradables.GetAllTradables()
            ));

            task.Start();
            state = State.UpdateTradables;
        }

        private void ProcessColonyTradablesReply(bool reply)
        {
            Logging.WriteMessage("Server accepted colony tradables");
            state = State.Done;
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
            Error,
        }

        #endregion
    }
}