#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, logging.cs 2021-02-03

#endregion

using System;
using System.Diagnostics;
using Verse;

namespace ise.lib
{
    internal static class Logging
    {
        #region Fields

        private const string Prefix = "ISE @ ";

        #endregion

        #region Methods

        private static void WriteLogMessage(string message, bool error, StackFrame sf)
        {
            // Go back two frames to get actual caller
            if (sf == null) sf = new StackTrace().GetFrame(2);
            var text = $"{Prefix}{DateTime.UtcNow} -> " +
                       $"{sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod().Name} -> {message}";

            if (error)
            {
                Log.Error(text);
            }
            else
            {
                if (IseCentral.Settings.DebugMessages) Log.Message(text);
            }
        }

        internal static void WriteErrorMessage(string message)
        {
            WriteLogMessage(message, true, null);
        }

        internal static void WriteDebugMessage(string message, StackFrame sf = null)
        {
            WriteLogMessage(message, false, sf);
        }

        internal static void WriteDebugMessage(bool condition, string message, StackFrame sf = null)
        {
            if (condition) WriteLogMessage(message, false, sf);
        }

        #endregion
    }
}