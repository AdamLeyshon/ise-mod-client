#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, task.cs 2021-02-11
// #endregion

#endregion

using ise.dialogs;

namespace ise.lib.tasks
{
    internal interface IDialogTask
    {
        #region Methods

        void Update();

        #endregion

        #region Properties

        IDialog Dialog { get; set; }

        bool Done { get; set; }

        #endregion
    }

    internal abstract class AbstractDialogTask : IDialogTask
    {
        #region ctor

        internal AbstractDialogTask(IDialog dialog)
        {
            Dialog = dialog;
        }

        #endregion

        #region IDialogTask Interface Implementations

        public bool Done { get; set; }

        public IDialog Dialog { get; set; }

        public virtual void Update()
        {
        }

        #endregion
    }
}