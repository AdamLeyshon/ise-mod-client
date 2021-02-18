#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, order.cs, Created 2021-02-18
#endregion

using System.Threading.Tasks;
using ise.dialogs;

namespace ise.lib.tasks
{
    internal class OrderPlaceDialogTask : AbstractDialogTask
    {
        private enum State
        {
            Start,
            Place,
            Done,
            Error,
        }

        private State state;
        private Task task;
        private string orderId;

        public OrderPlaceDialogTask(IDialog dialog) : base(dialog)
        {
            state = State.Start;
        }
    }
}