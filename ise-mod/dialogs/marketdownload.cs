#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, marketdownload.cs, Created 2021-02-12

#endregion

using ise.lib;
using ise.lib.tasks;
using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class DialogMarketDownload : Window, IDialog
    {
        #region Fields

        private readonly IDialogTask task;
        private Vector2 textWidth;

        #endregion

        #region ctor

        public DialogMarketDownload(Pawn userPawn)
        {
            Logging.WriteMessage("Opening Market Download Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            pawn = userPawn;
            task = new MarketDownloadDialogTask(this, pawn);
        }

        #endregion

        #region Properties

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

        #endregion

        #region IDialog Interface Implementations

        public string DialogMessage { get; set; }
        public Pawn pawn { get; set; }

        public void CloseDialog()
        {
            Close();
        }

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
            Find.WindowStack.Add(new DialogTradeUI(pawn));
            CloseDialog();
        }

        #endregion
    }
}