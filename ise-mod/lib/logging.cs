#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, logging.cs 2021-02-03

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Verse;

namespace ise.lib
{
    internal class Logging
    {
        #region Fields

        private const string Prefix = "ISE @ ";
        private static readonly Lazy<Logging> Instance = new Lazy<Logging>(() => new Logging());
        private readonly StreamWriter _stream;

        #endregion

        private Logging()
        {
            var logName = $"{GenFilePaths.ConfigFolderPath}/ISELog.txt";
            var file = new FileStream(logName, FileMode.Create, FileAccess.ReadWrite);
            _stream = new StreamWriter(file, Encoding.UTF8);
        }

        internal static Logging LoggerInstance => Instance.Value;

        #region Methods

        private void WriteLogMessage(string message, bool error, StackFrame sf)
        {
            // Go back two frames to get actual caller
            if (sf == null) sf = new StackTrace().GetFrame(2);
            var text = $"{Prefix}{DateTime.UtcNow} -> " +
                       $"{sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod().Name} -> {message}";

            if (error)
            {
                _stream.WriteLine(text);
                Log.Error(text);
            }
            else
            {
                if (!IseCentral.Settings.DebugMessages) return;
                _stream.WriteLine(text);
                Log.Message(text);
            }

            _stream.Flush();
        }

        internal void WriteErrorMessage(string message)
        {
            WriteLogMessage(message, true, null);
        }

        internal void WriteDebugMessage(string message, StackFrame sf = null)
        {
            WriteLogMessage(message, false, sf);
        }

        internal void WriteDebugMessage(bool condition, string message, StackFrame sf = null)
        {
            if (condition) WriteLogMessage(message, false, sf);
        }

        #endregion
    }
}