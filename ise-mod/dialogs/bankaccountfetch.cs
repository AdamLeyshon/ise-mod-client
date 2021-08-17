#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, bankaccountfetch.cs 2021-08-12

#endregion

using Common;
using ise.lib;
using ise.lib.tasks;
using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class DialogBankAccountFetch : Window, IDialog
    {
        #region ctor

        public DialogBankAccountFetch(Pawn userPawn, CurrencyEnum currency)
        {
            Logging.WriteDebugMessage("Preparing to get bank account data");
            _currency = currency;
            forcePause = true;
            absorbInputAroundWindow = true;
            Pawn = userPawn;
            _task = new BankAccountDialogTask(this, Pawn, _currency);
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

        #endregion

        #region Methods

        public override void DoWindowContents(Rect inRect)
        {
            _task.Update();

            _textWidth = Text.CalcSize(DialogMessage);

            var label = new Rect((inRect.width - _textWidth.x) / 2f, (inRect.height - _textWidth.y) / 2f,
                _textWidth.x, _textWidth.y);
            Widgets.Label(label, DialogMessage);

            if (!_task.Done) return;

            // Get the data back from the task
            var reply = ((BankAccountDialogTask)_task).Reply;

            if (reply == null) return;

            // Open withdrawal window
            Find.WindowStack.Add(new Atm(Pawn, reply, _currency));
            CloseDialog();
        }

        #endregion

        #region Fields

        private readonly IDialogTask _task;
        private Vector2 _textWidth;
        private readonly CurrencyEnum _currency;

        #endregion

        #region IDialog Interface Implementations

        public string DialogMessage { get; set; }
        public Pawn Pawn { get; set; }

        public void CloseDialog()
        {
            Close();
        }

        #endregion
    }
}