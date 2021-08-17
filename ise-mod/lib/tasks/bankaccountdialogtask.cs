#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, bankaccountdialogtask.cs 2021-08-12

#endregion

using System;
using System.Threading.Tasks;
using Bank;
using Common;
using ise.components;
using ise.dialogs;
using RestSharp;
using Verse;
using static ise_core.rest.Helpers;
using static ise_core.rest.api.v1.Constants;

namespace ise.lib.tasks
{
    internal class BankAccountDialogTask : AbstractDialogTask
    {
        #region ctor

        public BankAccountDialogTask(IDialog dialog, Pawn userPawn, CurrencyEnum currency) : base(dialog)
        {
            _currency = currency;
            _state = State.Start;
            _gc = Current.Game.GetComponent<ISEGameComponent>();
            _pawn = userPawn;
        }

        #endregion

        #region Properties

        public BankDataReply Reply { get; private set; }

        #endregion

        #region Nested type: State

        private enum State
        {
            Start,
            Fetch,
            Done,
            Error
        }

        #endregion

        #region Fields

        private readonly CurrencyEnum _currency;
        private readonly ISEGameComponent _gc;
        private readonly Pawn _pawn;
        private State _state;
        private Task _task;

        #endregion

        #region Methods

        public override void Update()
        {
            // Handle task errors first.
            if (_task != null && _task.IsFaulted) LogTaskError();

            switch (_state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    GetBankAccountData();
                    break;
                case State.Fetch:
                    Dialog.DialogMessage = "Communicating with bank";
                    if (_task != null && _task.IsCompleted) ProcessBankDataReply(((Task<BankDataReply>)_task).Result);

                    break;
                case State.Done:
                    Dialog.DialogMessage = "Bank account data received";
                    Done = true;
                    break;
                case State.Error:
                    Dialog.DialogMessage = "Could not read account data";
                    Done = true;
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

        private void GetBankAccountData()
        {
            var colonyId = _gc.GetColonyId(_pawn.Map);
            var request = new BankGetRequest { ClientBindId = _gc.ClientBind, ColonyId = colonyId };
            _task = SendAndParseReplyAsync(
                request,
                BankDataReply.Parser,
                $"{URLPrefix}bank/",
                Method.POST,
                _gc.ClientBind
            );
            _task.Start();
            _state = State.Fetch;
        }

        private void ProcessBankDataReply(BankDataReply reply)
        {
            if (reply == null || !reply.Balance.ContainsKey((int)_currency))
            {
                _state = State.Error;
                return;
            }

            Reply = reply;
            _state = State.Done;
        }

        #endregion
    }
}