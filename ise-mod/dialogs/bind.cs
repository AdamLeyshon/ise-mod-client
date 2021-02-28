#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, bind.cs, Created 2021-02-11

#endregion

using System;
using ise.lib;
using ise.lib.tasks;
using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class DialogBind : Window, IDialog
    {
        #region BindUIType enum

        internal enum BindUIType
        {
            Client,
            Colony,
            Account
        }

        #endregion

        #region Fields

        private readonly IDialogTask task;
        private Vector2 textWidth;

        #endregion

        #region ctor

        internal DialogBind(Pawn userPawn, BindUIType bindTypeEnum)
        {
            Logging.WriteMessage("Opening Bind Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            BindType = bindTypeEnum;
            Pawn = userPawn;
            switch (BindType)
            {
                case BindUIType.Client:
                    task = new ClientBindDialogTask(this);
                    break;
                case BindUIType.Colony:
                    task = new ColonyBindDialogTask(this, Pawn);
                    break;
                case BindUIType.Account:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Properties

        internal BindUIType BindType { get; set; }

        public override Vector2 InitialSize => new Vector2(300f, 300f);

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
            task.Update();

            textWidth = Text.CalcSize(DialogMessage);

            var label = new Rect((inRect.width - textWidth.x) / 2f, (inRect.height - textWidth.y) / 2f,
                textWidth.x, textWidth.y);
            Widgets.Label(label, DialogMessage);

            if (!task.Done) return;

            switch (BindType)
            {
                case BindUIType.Client:
                    Find.WindowStack.Add(new DialogBind(Pawn, BindUIType.Colony));
                    break;
                case BindUIType.Colony:
                    Find.WindowStack.Add(new DialogMarketDownload(Pawn));
                    break;
                case BindUIType.Account:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CloseDialog();
        }

        #endregion
    }
}