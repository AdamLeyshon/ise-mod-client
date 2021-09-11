#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, clientbinddialogtask.cs 2021-02-11

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
using static ise_core.rest.api.v1.Constants;

namespace ise.lib.tasks
{
    internal class ClientBindDialogTask : AbstractDialogTask
    {
        #region ctor

        public ClientBindDialogTask(IDialog dialog) : base(dialog)
        {
            _state = State.Start;
        }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Request,
            Confirm,
            WaitConfirm,
            Verify,
            Done,
            Error
        }

        #endregion

        #region Fields

        private string _bindId;

        private State _state;
        private Task _task;

        #endregion

        #region Methods

        public override void Update()
        {
            // Handle task errors first.
            if (_task != null && _task.IsFaulted) LogTaskError(_task);

            switch (_state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    if (_task == null) StartBind();
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Registering";
                    if (_task != null && _task.IsCompleted) ProcessBindRequestReply(((Task<BindReply>)_task).Result);

                    break;
                case State.Confirm:
                    Dialog.DialogMessage = "Confirming details";
                    if (_task != null && _task.IsCompleted)
                        ProcessConfirmBindReply(((Task<ConfirmBindReply>)_task).Result);

                    break;
                case State.WaitConfirm:
                    if (_task != null && _task.IsCompleted)
                    {
                        // Sleep over
                        StartConfirmBind();
                        _state = State.Confirm;
                    }

                    break;
                case State.Verify:
                    Dialog.DialogMessage = "Verifying details";
                    if (_task != null && _task.IsCompleted)
                        ProcessBindVerifyReply(((Task<ClientBindVerifyReply>)_task).Result);

                    break;
                case State.Done:
                    Dialog.DialogMessage = "Account OK";
                    Done = true;
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
            _state = State.Error;
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
                var request = new BindRequest { SteamId = IseCentral.User.UserId };
                _task = SendAndParseReplyAsync(
                    request,
                    BindReply.Parser,
                    $"{URLPrefix}binder/bind",
                    Method.POST
                );
                _task.Start();
                _state = State.Request;
            }
        }

        private void ProcessBindRequestReply(BindReply reply)
        {
            if (!reply.Valid)
            {
                Logging.WriteErrorMessage($"Server refused bind request: {reply}");
                _state = State.Error;
            }
            else
            {
                // Go to next step
                _bindId = reply.BindId;
                StartConfirmBind();
            }
        }

        private void StartConfirmBind()
        {
            _state = State.Confirm;
            var request = new ConfirmBindRequest { BindId = _bindId, BindType = BindTypeEnum.AccountBind };
            _task = SendAndParseReplyAsync(
                request,
                ConfirmBindReply.Parser,
                $"{URLPrefix}binder/bind_confirm",
                Method.POST
            );
            _task.Start();
        }

        private void ProcessConfirmBindReply(ConfirmBindReply reply)
        {
            if (reply.IsValid)
            {
                if (reply.BindComplete)
                    switch (reply.BindType)
                    {
                        case BindTypeEnum.AccountBind:
                            Logging.WriteDebugMessage($"Account bind confirmed: new Client Id: {reply.ClientBindId}");

                            // Now confirm the Client Bind ID
                            _state = State.Confirm;
                            var request = new ConfirmBindRequest
                                { BindId = reply.ClientBindId, BindType = BindTypeEnum.ClientBind };
                            _task = SendAndParseReplyAsync(
                                request,
                                ConfirmBindReply.Parser,
                                $"{URLPrefix}binder/bind_confirm",
                                Method.POST
                            );
                            _task.Start();
                            return;
                        case BindTypeEnum.ClientBind:
                            Logging.WriteDebugMessage($"Client bind confirmed: saving client Id: {reply.ClientBindId}");
                            var gc = Current.Game.GetComponent<ISEGameComponent>();
                            gc.ClientBind = reply.ClientBindId;
                            SaveBind<DBClientBind>(IseCentral.User.UserId, reply.ClientBindId);
                            StartBindVerify();
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                // Check if there's enough time left to go around again
                if (reply.TTL > 10)
                {
                    // Sleep for 10 seconds;
                    _task = new Task(delegate { Thread.Sleep(10 * 1000); });
                    _state = State.WaitConfirm;
                    Dialog.DialogMessage = $"Waiting for confirmation (About {reply.TTL}s left)";
                    _task.Start();
                    return;
                }
            }

            // Bind invalid, go back to start
            _bindId = "";
            Logging.WriteDebugMessage("Bind expired or was invalid, Requesting new bind");
            _task = null;
            _state = State.Start;
        }

        private void StartBindVerify()
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            Logging.WriteDebugMessage($"Verifying Client Bind {gc.ClientBind}");
            var request = new ClientBindVerifyRequest
            {
                SteamId = IseCentral.User.UserId,
                ClientBindId = gc.ClientBind
            };
            _task = SendAndParseReplyAsync(
                request,
                ClientBindVerifyReply.Parser,
                $"{URLPrefix}binder/bind_verify",
                Method.POST
            );
            _task.Start();
            _state = State.Verify;
        }

        private void ProcessBindVerifyReply(ClientBindVerifyReply reply)
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            if (!reply.Valid)
            {
                Logging.WriteErrorMessage($"Server refused bind: {reply.Valid}");

                // Delete the client bind stored
                DeleteBind<DBClientBind>(IseCentral.User.UserId);

                // Delete all colony binds :(
                DeleteBind<DBClientBind>(gc.ClientBind);
                gc.ClientBind = string.Empty;
                gc.ClientBindVerified = false;
                _task = null;
                _state = State.Start;
            }
            else
            {
                Logging.WriteDebugMessage($"Server accepted Bind {gc.ClientBind}");
                gc.ClientBindVerified = true;
                _state = State.Done;
            }
        }

        #endregion
    }
}