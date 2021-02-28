#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, dialog.cs, Created 2021-02-11

#endregion

using Verse;

namespace ise.dialogs
{
    internal interface IDialog
    {
        #region Properties

        string DialogMessage { get; set; }
        Pawn Pawn { get; set; }

        #endregion

        #region Methods

        void CloseDialog();

        #endregion
    }
}