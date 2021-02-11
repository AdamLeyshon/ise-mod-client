#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, client_bind.cs, Created 2021-02-11

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Bind;
using ise.components;
using ise.dialogs;
using ise_core.db;
using RestSharp;
using Verse;
using static ise_core.rest.Helpers;
using static ise.lib.User;
using static ise_core.rest.api.v1.consts;

namespace ise.lib.tasks
{
    internal class ClientBindDialogTask : AbstractDialogTask
    {
        private enum State
        {
            Start,
            Request,
            Confirm,
            WaitConfirm,
            Verify,
            Done,
            Error,
        }

        private State state;
        private Task task;
        private string bindId;

        public ClientBindDialogTask(IDialog dialog) : base(dialog)
        {
            state = State.Start;
        }

        public override void Update()
        {
            // Handle task errors first.
            if (task != null && task.IsFaulted)
            {
                LogTaskError(task);
            }

            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartBind();
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Registering";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessBindRequestReply(((Task<BindReply>) task).Result);
                    }

                    break;
                case State.Confirm:
                    Dialog.DialogMessage = "Confirming details";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessConfirmBindReply(((Task<ConfirmBindReply>) task).Result);
                    }

                    break;
                case State.WaitConfirm:
                    if (task != null && task.IsCompleted)
                    {
                        // Sleep over
                        StartConfirmBind();
                        state = State.Confirm;
                    }

                    break;
                case State.Verify:
                    Dialog.DialogMessage = "Verifying details";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessBindVerifyReply(((Task<ClientBindVerifyReply>) task).Result);
                    }

                    break;
                case State.Done:
                    Dialog.DialogMessage = "Account OK";
                    break;
                case State.Error:
                    Dialog.CloseDialog();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogTaskError(Task task)
        {
            state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {task.Exception}");
            task = null;
        }

        private void StartBind()
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            if (!gc.ClientBind.NullOrEmpty())
            {
                // Jump straight to verify
                StartBindVerify();
            }
            else
            {
                var request = new BindRequest() {SteamId = IseBootStrap.User.UserId};
                task = SendAndParseReplyAsync(
                    request,
                    BindReply.Parser,
                    $"{URLPrefix}binder/bind",
                    Method.POST
                );
                task.Start();
                state = State.Request;
            }
        }

        private void ProcessBindRequestReply(BindReply reply)
        {
            if (!reply.Valid)
            {
                Logging.WriteErrorMessage($"Server refused bind request: {reply}");
                state = State.Error;
            }
            else
            {
                // Go to next step
                bindId = reply.BindId;
                StartConfirmBind();
            }
        }

        private void StartConfirmBind()
        {
            state = State.Confirm;
            var request = new ConfirmBindRequest() {BindId = bindId, BindType = BindTypeEnum.AccountBind};
            task = SendAndParseReplyAsync(
                request,
                ConfirmBindReply.Parser,
                $"{URLPrefix}binder/bind_confirm",
                Method.POST
            );
            task.Start();
        }

        private void ProcessConfirmBindReply(ConfirmBindReply reply)
        {
            if (reply.IsValid)
            {
                if (reply.BindComplete)
                {
                    switch (reply.BindType)
                    {
                        case BindTypeEnum.AccountBind:


                            Logging.WriteMessage($"Account bind confirmed: new Client Id: {reply.ClientBindId}");

                            // Now confirm the Client Bind ID
                            state = State.Confirm;
                            var request = new ConfirmBindRequest()
                                {BindId = reply.ClientBindId, BindType = BindTypeEnum.ClientBind};
                            task = SendAndParseReplyAsync(
                                request,
                                ConfirmBindReply.Parser,
                                $"{URLPrefix}binder/bind_confirm",
                                Method.POST
                            );
                            task.Start();
                            return;
                        case BindTypeEnum.ClientBind:
                            Logging.WriteMessage($"Client bind confirmed: saving client Id: {reply.ClientBindId}");
                            var gc = Current.Game.GetComponent<ISEGameComponent>();
                            gc.ClientBind = reply.ClientBindId;
                            SaveBind<DBClientBind>(IseBootStrap.User.UserId, reply.ClientBindId);
                            StartBindVerify();
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // Check if there's enough time left to go around again
                if (reply.TTL > 10)
                {
                    // Sleep for 10 seconds;
                    task = new Task(delegate { Thread.Sleep(10 * 1000); });
                    state = State.WaitConfirm;
                    Dialog.DialogMessage = $"Waiting for confirmation (About {reply.TTL}s left)";
                    task.Start();
                    return;
                }
            }

            // Bind invalid, go back to start
            bindId = "";
            Logging.WriteMessage("Bind expired or was invalid, Requesting new bind");
            task = null;
            state = State.Start;
        }

        private void StartBindVerify()
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            Logging.WriteMessage($"Verifying Client Bind {gc.ClientBind}");
            var request = new ClientBindVerifyRequest()
            {
                SteamId = IseBootStrap.User.UserId,
                ClientBindId = gc.ClientBind
            };
            task = SendAndParseReplyAsync(
                request,
                ClientBindVerifyReply.Parser,
                $"{URLPrefix}binder/bind_verify",
                Method.POST
            );
            task.Start();
            state = State.Verify;
        }

        private void ProcessBindVerifyReply(ClientBindVerifyReply reply)
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            if (!reply.Valid)
            {
                Logging.WriteErrorMessage($"Server refused bind: {reply.Valid}");
                task = null;
                state = State.Start;
                gc.BindVerified = false;
            }
            else
            {
                Logging.WriteMessage($"Server accepted Bind {gc.ClientBind}");
                gc.BindVerified = true;
                state = State.Done;
            }
        }
    }
}