#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, atm.cs 2021-08-12
// #endregion

#endregion

using System;
using Bank;
using Common;
using ise.components;
using ise.lib;
using ise_core.db;
using ise_core.rest;
using Order;
using RestSharp;
using UnityEngine;
using Verse;
using static ise_core.rest.api.v1.Constants;

namespace ise.dialogs
{
    public class Atm : Window, IDialog
    {
        #region ctor

        public Atm(Pawn userPawn, BankDataReply bankData, CurrencyEnum currency)
        {
            _currency = currency;
            Logging.WriteDebugMessage("Opening ATM Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            Pawn = userPawn;
            _fundsAvailable = bankData.Balance[(int)_currency];
            DialogMessage = "ISEWithdrawStatement".Translate(new NamedArgument(_fundsAvailable, "amount"));
            _gc = Current.Game.GetComponent<ISEGameComponent>();
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

        #endregion

        #region Fields

        private readonly CurrencyEnum _currency;
        private readonly int _fundsAvailable;
        private int _fundsWithdrawn;
        private string _textBuffer = "";
        private readonly ISEGameComponent _gc;

        #endregion

        #region IDialog Interface Implementations

        public string DialogMessage { get; set; }
        public Pawn Pawn { get; set; }

        public void CloseDialog()
        {
            Close();
        }

        #endregion

        #region Methods

        public override void DoWindowContents(Rect inRect)
        {
            const float padding = 6f;
            const float buttonHeight = 25f;
            const float buttonWidth = 100f;

            var width = inRect.width;
            var height = 0f;

            GUI.BeginGroup(inRect);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Close button
            width -= buttonWidth + padding;
            var rect = new Rect(
                width,
                height,
                buttonWidth,
                buttonHeight);
            if (Widgets.ButtonText(rect, "Close")) CloseDialog();

            // Withdraw button
            width -= buttonWidth + padding;
            rect = new Rect(
                width,
                height,
                buttonWidth,
                buttonHeight);
            if (Widgets.ButtonText(rect, "Withdraw")) ProcessWithdrawal();

            // Numerical input box
            Text.Anchor = TextAnchor.MiddleCenter;
            rect = new Rect(0, buttonHeight + (padding * 2), inRect.width - padding, buttonHeight);
            Widgets.TextFieldNumeric(rect, ref _fundsWithdrawn, ref _textBuffer, 1f, _fundsAvailable);

            // Dialog message
            rect = new Rect(
                padding,
                padding,
                inRect.width - padding * 2,
                inRect.height - buttonHeight - padding * 2);

            Widgets.Label(rect, DialogMessage);

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void ProcessWithdrawal()
        {
            if (_fundsWithdrawn > _fundsAvailable || _fundsWithdrawn <= 0f)
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "ISEWithdrawInvalidAmount".Translate(),
                    "OK",
                    null,
                    title: "Invalid amount"
                ));
                return;
            }

            string dialogText;
            var colonyId = _gc.GetColonyId(Pawn.Map);
            var request = new BankWithdrawRequest
            {
                ClientBindId = _gc.ClientBind,
                ColonyId = colonyId,
                Currency = _currency,
                Amount = _fundsWithdrawn
            };

            try
            {
                var reply = Helpers.SendAndParseReply(request, BankWithdrawReply.Parser,
                    $"{URLPrefix}bank/withdraw", Method.POST, _gc.ClientBind);

                if (reply.Status != OrderRequestStatus.AcceptedAll)
                    throw new ApplicationException($"Server refused transaction: {reply}");

                // Add to database
                IseCentral.DataCache.GetCollection<DBOrder>(Constants.Tables.Orders).Insert(
                    new DBOrder
                    {
                        Id = reply.Data.OrderId,
                        ColonyId = colonyId,
                        Status = reply.Data.Status,
                        PlacedTick = reply.Data.PlacedTick,
                        DeliveryTick = reply.Data.DeliveryTick
                    });

                // Start the order manager
                _gc.GetAccount(colonyId).AddOrder(reply.Data.OrderId);

                dialogText = "ISEWithdrawComplete".Translate();
            }
            catch (Exception e)
            {
                Logging.WriteErrorMessage($"{e}");
                dialogText = "ISEWithdrawFailure".Translate();
            }

            Find.WindowStack.Add(new Dialog_MessageBox(
                dialogText,
                "OK",
                CloseDialog,
                title: "Transaction Result"
            ));
        }

        #endregion
    }
}