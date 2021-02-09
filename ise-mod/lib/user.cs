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

        internal static DBUserData LoadUserData()
        {
            var steamId = GetUserSteamId();

            using (var db = new LiteDatabase(DBLocation))
            {
                Logging.WriteMessage($"Loading from {db}");
                var col = db.GetCollection<DBUserData>("user_data");

                var userData = steamId.NullOrEmpty()
                    ? col.FindOne(data => data.IsSteamUser == false)
                    : col.FindOne(data => data.IsSteamUser && data.UserId == steamId);

                if (userData == null)
                {
                    Logging.WriteMessage($"Creating new user data");
                    userData = steamId.NullOrEmpty()
                        ? new DBUserData() {UserId = Guid.NewGuid().ToString(), IsSteamUser = false}
                        : new DBUserData() {UserId = steamId, IsSteamUser = true};
                    col.Insert(userData);
                }

                Logging.WriteMessage($"Loaded User ID: {userData.UserId}");
                return userData;
            }
        }

        internal static string LoadClientBind(string userId)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                // Get the collection (or create, if doesn't exist)
                var col = db.GetCollection<DBClientBindData>("client_bind");

                var clientBindData = col.FindOne(data => data.UserId == userId);

                if (clientBindData == null) return string.Empty;

                Logging.WriteMessage($"Loaded Bind ID: {clientBindData.ClientBindId}");
                return clientBindData.ClientBindId;
            }
        }

        internal static void SaveClientBind(string userId, string clientBindId)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                // Get the collection (or create, if doesn't exist)
                var col = db.GetCollection<DBClientBindData>("client_bind");

                var clientBind = new DBClientBindData() {UserId = userId, ClientBindId = clientBindId};
                col.Insert(clientBind);
            }
        }
    }
}