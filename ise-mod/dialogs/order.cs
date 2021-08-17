#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, order.cs 2021-02-18

#endregion

using System;
using ise.components;
using ise.lib;
using ise.lib.tasks;
using ise_core.db;
using Order;
using RimWorld;
using UnityEngine;
using Verse;
using static ise.lib.Cache;

namespace ise.dialogs
{
    public class DialogOrder : Window, IDialog
    {
        #region ctor

        public DialogOrder(Pawn userPawn, int additionalFunds)
        {
            Logging.WriteDebugMessage("Opening Place Order Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            Pawn = userPawn;
            _task = new OrderPlaceDialogTask(this, Pawn, additionalFunds);
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

        #endregion

        #region Fields

        private readonly IDialogTask _task;
        private Vector2 _messageScrollbar = Vector2.zero;
        private bool _processedReply;
        private Vector2 _textWidth;

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
            Rect label;
            const float padding = 6f;
            const float buttonHeight = 25f;
            const float buttonWidth = 100f;
            if (!_task.Done)
            {
                _textWidth = Text.CalcSize(DialogMessage);
                label = new Rect((inRect.width - _textWidth.x) / 2f, (inRect.height - _textWidth.y) / 2f,
                    _textWidth.x, _textWidth.y);
                Widgets.Label(label, DialogMessage);
                _task.Update();
                return;
            }

            if (!_processedReply)
            {
                var orderTask = (OrderPlaceDialogTask)_task;
                if (orderTask.Reply?.Data == null)
                {
                    Close();
                    return;
                }

                TaskComplete();
            }

            // Show buttons to close and final message regarding order status
            GUI.BeginGroup(inRect);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Confirm button

            var buttonRect = new Rect(
                inRect.width - buttonWidth - padding,
                inRect.height - buttonHeight - padding,
                buttonWidth,
                buttonHeight);
            if (Widgets.ButtonText(buttonRect, "Close")) CloseDialog();

            label = new Rect(
                padding,
                padding,
                inRect.width - padding * 2,
                inRect.height - buttonHeight - padding * 2);
            Widgets.LabelScrollable(label, DialogMessage, ref _messageScrollbar, longLabel: false);

            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        private void TaskComplete()
        {
            var gc = Current.Game.GetComponent<ISEGameComponent>();
            var colonyId = gc.GetColonyId(Pawn.Map);
            int deliveryTime;
            var orderTask = (OrderPlaceDialogTask)_task;
            switch (orderTask.Reply.Status)
            {
                case OrderRequestStatus.Rejected:
                    DialogMessage = "We were unable to place your order, \r\n" +
                                    "There are a number of reasons for this, \r\n" +
                                    "Please check your computers clock is accurate and \r\n" +
                                    "that your mod is up to date.";
                    break;
                case OrderRequestStatus.AcceptedAll:
                    deliveryTime = orderTask.Reply.Data.DeliveryTick - orderTask.Reply.Data.PlacedTick;
                    IseCentral.DataCache.GetCollection<DBInventoryPromise>().DeleteMany(x => x.ColonyId == colonyId);

                    DialogMessage = "Your order was successfully placed and your payment, \r\n" +
                                    "has been processed.\r\n\r\n" +
                                    "Your order should arrive in approximately " +
                                    $"{deliveryTime.ToStringTicksToDays()}\r\n\r\n" +
                                    "Thank you for using Interstellar Express";
                    break;
                case OrderRequestStatus.AcceptedPartial:
                    deliveryTime = orderTask.Reply.Data.DeliveryTick - orderTask.Reply.Data.PlacedTick;
                    IseCentral.DataCache.GetCollection<DBInventoryPromise>().DeleteMany(x => x.ColonyId == colonyId);
                    var message = "Your order was successfully placed and your payment, \r\n" +
                                  "has been processed.\r\n\r\n" +
                                  "However, some of the items you requested were not in stock\r\n" +
                                  "and your account has been refunded the amount\r\n\r\n" +
                                  "Your order should arrive in approximately " +
                                  $"{deliveryTime.ToStringTicksToDays()}\r\n\r\n" +
                                  "Thank you for using Interstellar Express\r\n\r\n" +
                                  "The items we were unable to deliver are:\r\n";

                    var cache = GetCache(colonyId, CacheType.MarketCache);
                    foreach (var orderItem in orderTask.Reply.Unavailable)
                    {
                        var def = DefDatabase<ThingDef>.GetNamed(cache.FindById(orderItem.ItemCode).ThingDef);
                        message += $"{def.LabelCap} x {orderItem.Quantity}\r\n";
                    }

                    DialogMessage = message;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _processedReply = true;
        }

        #endregion
    }
}