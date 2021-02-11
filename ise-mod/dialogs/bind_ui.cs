#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, bind_ui.cs, Created 2021-02-11

#endregion

using System;
using Bind;
using ise.lib.tasks;
using UnityEngine;
using Verse;

namespace ise.dialogs
{
    public class BindUI : Window, IDialog
    {
        private Vector2 textWidth;
        private IDialogTask task;

        public enum BindUIType
        {
            Client,
            Colony,
        }

        public BindUI(Pawn userPawn, BindUIType bindTypeEnum)
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            BindType = bindTypeEnum;
            switch (BindType)
            {
                case BindUIType.Client:
                    task = new ClientBindDialogTask(this);
                    break;
                case BindUIType.Colony:
                    task = new ClientBindDialogTask(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public BindUIType BindType { get; set; }

        public override Vector2 InitialSize =>
            new Vector2(300f, 300f);

        public override void DoWindowContents(Rect inRect)
        {
            task.Update();
            
            textWidth = Text.CalcSize(DialogMessage);

            var label = new Rect((inRect.width - textWidth.x) / 2f, (inRect.height - textWidth.y) / 2f,
                textWidth.x, textWidth.y);
            Widgets.Label(label, DialogMessage);
        }

        public string DialogMessage { get; set; }
        public void CloseDialog()
        {
            this.Close();
        }
    }
}