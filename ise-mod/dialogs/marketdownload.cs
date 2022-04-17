#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, marketdownload.cs 2021-02-12

#endregion

using ise.lib;
using ise.lib.tasks;
using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class DialogMarketDownload : Window, IDialog
    {
        #region ctor

        public DialogMarketDownload(Pawn userPawn)
        {
            Logging.WriteDebugMessage("Opening Market Download Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            Pawn = userPawn;
            task = new MarketDownloadDialogTask(this, Pawn);
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

        #endregion

        #region Methods

        public override void DoWindowContents(Rect inRect)
        {
            task.Update();

            textWidth = Text.CalcSize(DialogMessage);

            var label = new Rect((inRect.width - textWidth.x) / 2f, (inRect.height - textWidth.y) / 2f,
                textWidth.x, textWidth.y);
            Widgets.Label(label, DialogMessage);

            if (!task.Done) return;

            // Open market window
            Find.WindowStack.Add(new DialogTradeUI(Pawn));
            CloseDialog();
        }

        #endregion

        #region Fields

        private readonly IDialogTask task;
        private Vector2 textWidth;

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