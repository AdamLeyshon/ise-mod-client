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

        internal static void WriteErrorMessage(string message)
        {
            var sf = new StackTrace().GetFrame(1);
            WriteErrorMessage(message, sf);
        }

        private static void WriteErrorMessage(string message, StackFrame sf)
        {
            Log.Error($"{Prefix}{DateTime.UtcNow} -> " +
                      $"{sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod().Name} -> {message}");
        }

        internal static void WriteMessage(string message, StackFrame sf = null)
        {
            if (sf == null) sf = new StackTrace().GetFrame(1);

            Log.Message($"{Prefix}{DateTime.UtcNow} -> " +
                        $"{sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod().Name} -> {message}");
        }

        internal static void WriteDebugMessage(string message, StackFrame sf = null)
        {
#if DEBUG
            if (sf == null) sf = new StackTrace().GetFrame(1);
            WriteMessage(message, sf);
#endif
        }

        #endregion
    }
}