#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, client_bind.cs, Created 2021-02-11

#endregion

using System;
using ise.dialogs;

namespace ise.lib.tasks
{
    internal class ColonyBindDialogTask : AbstractDialogTask
    {
        private enum State
        {
            Start = 0,
            Request = 1,
            Verify = 2,
            Done = 3,
        }

        private State state;

        public ColonyBindDialogTask(IDialog dialog) : base(dialog)
        {
            state = State.Start;
        }

        public override void Update()
        {
            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Linking this Colony to account";
                    break;
                case State.Verify:
                    Dialog.DialogMessage = "Verifying link";
                    break;
                case State.Done:
                    Dialog.DialogMessage = "Colony link complete";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}