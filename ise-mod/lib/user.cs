#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, user.cs, Created 2021-02-09

#endregion

using System;
using ise_core.db;
using LiteDB;
using Steamworks;
using Verse;
using Verse.Steam;
using static ise.lib.consts;

namespace ise.lib
{
    internal static class User
    {
        private static string GetUserSteamId()
        {
            return SteamManager.Initialized ? SteamUser.GetSteamID().ToString() : null;
        }

        internal static DBUser LoadUserData()
        {
            var steamId = GetUserSteamId();

            using (var db = new LiteDatabase(DBLocation))
            {
                Logging.WriteMessage($"Loading from {db}");
                var col = db.GetCollection<DBUser>("user_data");

                var userData = steamId.NullOrEmpty()
                    ? col.FindOne(data => data.IsSteamUser == false)
                    : col.FindOne(data => data.IsSteamUser && data.UserId == steamId);

                if (userData == null)
                {
                    Logging.WriteMessage($"Creating new user data");
                    userData = steamId.NullOrEmpty()
                        ? new DBUser() {UserId = Guid.NewGuid().ToString(), IsSteamUser = false}
                        : new DBUser() {UserId = steamId, IsSteamUser = true};
                    col.Insert(userData);
                }

                Logging.WriteMessage($"Loaded User ID: {userData.UserId}");
                return userData;
            }
        }

        internal static string LoadBind<T>(string userId) where T : IBaseBind, new()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                // Get the collection (or create, if doesn't exist)
                var col = db.GetCollection<T>("bindings");

                var clientBindData = col.FindOne(data => data.BindId == userId);

                if (clientBindData == null) return string.Empty;

                Logging.WriteMessage($"Loaded {typeof(T)} with ID: {clientBindData.BindId}");
                return clientBindData.BindId;
            }
        }

        internal static void SaveBind<T>(string userId, string bindId) where T : IBaseBind, new()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                // Get the collection (or create, if doesn't exist)
                var col = db.GetCollection<T>("bindings");

                var bind = new T() {UserId = userId, BindId = bindId};
                col.Insert(bind);
            }
        }
    }
}