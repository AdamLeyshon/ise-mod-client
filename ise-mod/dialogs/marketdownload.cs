#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, market_download.cs, Created 2021-02-12

#endregion

using ise.lib;
using ise.lib.tasks;
using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class DialogMarketDownload : Window, IDialog
    {
        private Vector2 textWidth;
        private readonly IDialogTask task;

        public DialogMarketDownload(Pawn userPawn)
        {
            Logging.WriteMessage("Opening Market Download Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            pawn = userPawn;
            task = new MarketDownloadDialogTask(this, pawn);
        }

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

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

        public string DialogMessage { get; set; }
        public Pawn pawn { get; set; }

        public void CloseDialog()
        {
            this.Close();
        }
    }
}