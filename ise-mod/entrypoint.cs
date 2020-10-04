using Hello;
using ise_core.db;
using LiteDB;
using RimWorld;
using UnityEngine;
using Verse;

namespace ise
{
    [StaticConstructorOnStartup]
    public class ISEBootStrap
    {
        static ISEBootStrap()
        {
            LoadOrCreateUserData();

            var send = new HelloRequest() {ClientVersion = "1.0"};
            var client = ise_core.rest.Helpers.CreateRestClient();
            var reply = ise_core.rest.Helpers.SendAndReply<HelloReply, HelloRequest>(client, send, HelloReply.Parser,
                "/api/v1/player/");
            Log.Message($"Got message from server: {reply.ServerVersion}");
        }

        // Normally we'd put this in the state tracker.
        // But I want to see if RW can actually run this.
        private static void LoadOrCreateUserData()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"./ISEData.db"))
            {
                // Get the collection (or create, if doesn't exist)
                var col = db.GetCollection<DBUserData>("userdata");

                var existingUserData = col.FindById(1);

                if (existingUserData != null)
                {
                    Log.Message($"Loaded user ID: {existingUserData.UserId}");
                }

                // We need to create one when we first talk to the server.
                // For now let's just insert a test row.

                var customer = new DBUserData
                {
                    UserName = "Test User",
                    UserId = "12345ABCDE"
                };

                // Insert new customer document (Id will be auto-incremented)
                col.Insert(customer);
            }
        }
    }
}