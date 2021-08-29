#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, entrypoint.cs 2020-09-02

#endregion

using System;
using System.Reflection;
using System.Threading.Tasks;
using ise.lib;
using ise_core.db;
using ise_core.rest.api.v1;
using LiteDB;
using Verse;
using static ise.lib.User;
using static ise.lib.Constants;
using static ise_core.rest.api.v1.System;

namespace ise
{
    [StaticConstructorOnStartup]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IseCentral
    {
        #region Fields

        internal static readonly DBUser User;
        internal static readonly string ModVersion;
        internal static bool HandshakeComplete;
        internal static DateTime LastHandshakeAttempt;
        internal static ServerState ISEState;
        internal static Task HandshakeTask;

        #endregion

        public enum ServerState
        {
            HandshakeNotPerformed,
            Unreachable,
            Maintenance,
            ClientTooOld,
            OK
        }

        #region ctor

        static IseCentral()
        {
            ModVersion = GetModVersion();
            ise_core.rest.Helpers.UserAgentVersion = $"ISE/{ModVersion}";
            DataCache = new LiteDatabase(DBLocation);
            User = LoadUserData();
            HandshakeComplete = false;
            ISEState = ServerState.HandshakeNotPerformed;
            LastHandshakeAttempt = DateTime.MinValue;
            HandshakeTask = null;
        }

        private static string GetModVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        internal static void StartHandshakeTask()
        {
            if (HandshakeTask != null && !HandshakeTask.IsCompleted) return;
            HandshakeTask = DoHandshake();
            HandshakeTask.Start();
        }

        internal static Task DoHandshake()
        {
            return new Task(
                () =>
                {
                    ISEState = ServerState.HandshakeNotPerformed;
                    HandshakeComplete = false;

                    try
                    {
                        var reply = Handshake();
                        Logging.WriteMessage($"Server online, version {reply.ServerVersion}");

                        switch (reply.ResultCode)
                        {
                            case HandshakeResult.Unreachable:
                                ISEState = ServerState.Unreachable;
                                break;
                            case HandshakeResult.ClientOutdated:
                                ISEState = ServerState.ClientTooOld;
                                break;
                            case HandshakeResult.UnderMaintenance:
                                ISEState = ServerState.Maintenance;
                                break;
                            case HandshakeResult.OK:
                                ISEState = ServerState.OK;
                                HandshakeComplete = true;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception e)
                    {
                        ISEState = ServerState.HandshakeNotPerformed;
                    }

                    LastHandshakeAttempt = DateTime.Now;
                });
        }

        #endregion

        #region Properties

        internal static LiteDatabase DataCache { get; }

        #endregion
    }
}