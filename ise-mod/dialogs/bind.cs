#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, bind.cs 2021-02-11

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
        #region ctor

        internal DialogBind(Pawn userPawn, BindUIType bindTypeEnum, Window destination)
        {
            Logging.WriteDebugMessage("Opening Bind Dialog");
            forcePause = true;
            absorbInputAroundWindow = true;
            BindType = bindTypeEnum;
            Pawn = userPawn;
            _destination = destination;
            switch (BindType)
            {
                case BindUIType.Client:
                    _task = new ClientBindDialogTask(this);
                    break;
                case BindUIType.Colony:
                    _task = new ColonyBindDialogTask(this, Pawn);
                    break;
                case BindUIType.Account:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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

            switch (BindType)
            {
                case BindUIType.Client:
                    Find.WindowStack.Add(new DialogBind(Pawn, BindUIType.Colony, _destination));
                    break;
                case BindUIType.Colony:
                    Find.WindowStack.Add(_destination);
                    break;
                case BindUIType.Account:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CloseDialog();
        }

        #endregion

        #region BindUIType enum

        internal enum BindUIType
        {
            Client,
            Colony,
            Account
        }

        #endregion

        #region Fields

        private readonly IDialogTask _task;
        private Vector2 _textWidth;
        private readonly Window _destination;

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
    }
}