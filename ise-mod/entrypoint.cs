using System;
using ise_core.db;
using LiteDB;
using RestSharp;
using RimWorld;
using UnityEngine;
using Verse;
using ise.lib;

namespace ise
{
    [StaticConstructorOnStartup]
    public class IseBootStrap
    {
        public const string TestClientBindID = "0IFEmQt8MWmmoK38qn";
        public const string TestClientSteamID = "[U:1:13457876]";
        public const string TestColonyID = "6Y1ebXmewlqaSCLmvCp7tpgWcdaEbRBN";

        static IseBootStrap()
        {
            // LoadOrCreateUserData();
            try
            {
                lib.API.v1.System.Hello();
            }
            catch (Exception _)
            {
                Logging.LogWriter.WriteErrorMessage("Hello Failed, API unavailable");
            }
            Logging.LogWriter.WriteMessage("Sending mods to Server");
            try
            {
                ise_core.rest.api.v1.colony.Data.SetModList(
                    Mods.GetModList(),
                    TestClientBindID,
                    TestColonyID
                );
            }
            catch (Exception _)
            {
                Logging.LogWriter.WriteErrorMessage("API did not accept mod list");
            }
            Logging.LogWriter.WriteMessage("Done");
            Logging.LogWriter.WriteMessage("Sending tradable items to Server");
            try
            {
                ise_core.rest.api.v1.colony.Data.SetTradablesList(
                    Tradables.GetAllTradables(),
                    TestClientBindID,
                    TestColonyID
                );
            }
            catch (Exception _)
            {
                Logging.LogWriter.WriteErrorMessage("API did not accept tradable list");
            }
            Logging.LogWriter.WriteMessage("Done");
            
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