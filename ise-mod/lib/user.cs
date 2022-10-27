#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-mod, user.cs 2021-08-16

#endregion

using System;
using ise_core.db;
using Steamworks;
using Verse;
using Verse.Steam;
using static ise.lib.Constants;

namespace ise.lib
{
    internal static class User
    {
        #region Methods

        private static string GetUserSteamId()
        {
// #if DEBUG
//             // Pretend we're a steam user
//             return "[U:1:13457876]";
// #else
            return SteamManager.Initialized ? SteamUser.GetSteamID().ToString() : null;
// #endif
        }

        internal static DBUser LoadUserData()
        {
            var steamId = GetUserSteamId();
            var db = IseCentral.DataCache;
            Logging.LoggerInstance.WriteDebugMessage("Trying to load user data");
            var col = db.GetCollection<DBUser>(Tables.Users);

            var userData = steamId.NullOrEmpty()
                ? col.FindOne(data => data.IsSteamUser == false)
                : col.FindOne(data => data.IsSteamUser && data.UserId == steamId);

            if (userData == null)
            {
                Logging.LoggerInstance.WriteDebugMessage("Creating new user data");
                userData = steamId.NullOrEmpty()
                    ? new DBUser { UserId = Guid.NewGuid().ToString(), IsSteamUser = false }
                    : new DBUser { UserId = steamId, IsSteamUser = true };
                col.Insert(userData);
            }

            Logging.LoggerInstance.WriteDebugMessage($"Loaded User ID: {userData.UserId}");
            return userData;
        }

        internal static string LoadBind<T>(string parentId) where T : IBaseBind, new()
        {
            var db = IseCentral.DataCache;
            var col = db.GetCollection<T>(Tables.Bindings);
            var clientBindData = col.FindOne(data => data.ParentId == parentId);
            if (clientBindData == null) return string.Empty;
            Logging.LoggerInstance.WriteDebugMessage($"Loaded {typeof(T)} with ID: {clientBindData.BindId}");
            return clientBindData.BindId;
        }

        internal static void DeleteBind<T>(string targetBind) where T : IBaseBind, new()
        {
            var db = IseCentral.DataCache;
            var col = db.GetCollection<T>(Tables.Bindings);
            col.DeleteMany(data => data.BindId == targetBind);
            Logging.LoggerInstance.WriteDebugMessage($"Deleted all binds referring to {typeof(T)} with ID: {targetBind}");
        }

        internal static void SaveBind<T>(string parentId, string bindId) where T : IBaseBind, new()
        {
            var db = IseCentral.DataCache;
            var col = db.GetCollection<T>(Tables.Bindings);
            var bind = new T { ParentId = parentId, BindId = bindId };
            col.Insert(bind);
        }

        #endregion
    }
}