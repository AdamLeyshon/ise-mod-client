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
using ise.settings;
using ise_core.db;
using ise_core.rest;
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
        public enum ServerState
        {
            HandshakeNotPerformed,
            Unreachable,
            Maintenance,
            ClientTooOld,
            OK
        }

        #region Properties

        internal static LiteDatabase DataCache { get; }

        #endregion

        #region Fields

        internal static readonly DBUser User;
        internal static readonly string ModVersion;
        internal static ISESettings Settings;
        internal static bool HandshakeComplete;
        internal static DateTime LastHandshakeAttempt;
        internal static ServerState ISEState;
        internal static Task HandshakeTask;

        #endregion

        #region ctor

        static IseCentral()
        {
            ReloadSettings();
            ModVersion = GetModVersion();
            Helpers.UserAgentVersion = $"ISE/{ModVersion}";
            DataCache = new LiteDatabase(DBLocation);
            User = LoadUserData();
            HandshakeComplete = false;
            ISEState = ServerState.HandshakeNotPerformed;
            LastHandshakeAttempt = DateTime.MinValue;
            HandshakeTask = null;
        }

        public static void ReloadSettings()
        {
            Settings = LoadedModManager.GetMod<ISEMod>().GetSettings<ISESettings>();
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
                    var version = "";
                    try
                    {
                        var reply = Handshake();

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

                        if (!reply.ServerVersion.NullOrEmpty()) version = reply.ServerVersion;
                    }
                    catch (Exception)
                    {
                        ISEState = ServerState.HandshakeNotPerformed;
                    }

                    if (ISEState == ServerState.Unreachable || ISEState == ServerState.HandshakeNotPerformed)
                        Logging.WriteDebugMessage("Server offline");
                    else
                        Logging.WriteDebugMessage($"Server online ({ISEState}), version {version}");

                    LastHandshakeAttempt = DateTime.Now;
                });
        }

        #endregion
    }
}