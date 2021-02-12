#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, task.cs, Created 2021-02-11

#endregion

using ise.dialogs;

namespace ise.lib.tasks
{
    public interface IDialogTask
    {
        IDialog Dialog { get; set; }

        bool Done { get; set; }

        void Update();
    }

    internal abstract class AbstractDialogTask : IDialogTask
    {
        public AbstractDialogTask(IDialog dialog)
        {
            Dialog = dialog;
        }

        public bool Done { get; set; }

        public IDialog Dialog { get; set; }

        public virtual void Update()
        {
        }
    }
}