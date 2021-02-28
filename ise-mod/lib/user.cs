#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, user.cs, Created 2021-02-09

#endregion

using System;
using ise_core.db;
using Verse;
using static ise.lib.Constants;

namespace ise.lib
{
    internal static class User
    {
        #region Methods

        private static string GetUserSteamId()
        {
#if DEBUG
            // Pretend we're a steam user
            return "[U:1:13457876]";
#else
            return SteamManager.Initialized ? SteamUser.GetSteamID().ToString() : null;
#endif
        }

        internal static DBUser LoadUserData()
        {
            var steamId = GetUserSteamId();
            var db = IseCentral.DataCache;
            Logging.WriteMessage($"Trying to load user data");
            var col = db.GetCollection<DBUser>(Tables.Users);

            var userData = steamId.NullOrEmpty()
                ? col.FindOne(data => data.IsSteamUser == false)
                : col.FindOne(data => data.IsSteamUser && data.UserId == steamId);

            if (userData == null)
            {
                Logging.WriteMessage("Creating new user data");
                userData = steamId.NullOrEmpty()
                    ? new DBUser {UserId = Guid.NewGuid().ToString(), IsSteamUser = false}
                    : new DBUser {UserId = steamId, IsSteamUser = true};
                col.Insert(userData);
            }

            Logging.WriteMessage($"Loaded User ID: {userData.UserId}");
            return userData;
        }

        internal static string LoadBind<T>(string parentId) where T : IBaseBind, new()
        {
            var db = IseCentral.DataCache;
            var col = db.GetCollection<T>(Tables.Bindings);
            var clientBindData = col.FindOne(data => data.ParentId == parentId);
            if (clientBindData == null) return string.Empty;
            Logging.WriteMessage($"Loaded {typeof(T)} with ID: {clientBindData.BindId}");
            return clientBindData.BindId;
        }

        internal static void SaveBind<T>(string parentId, string bindId) where T : IBaseBind, new()
        {
            var db = IseCentral.DataCache;
            var col = db.GetCollection<T>(Tables.Bindings);
            var bind = new T {ParentId = parentId, BindId = bindId};
            col.Insert(bind);
        }

        #endregion
    }
}