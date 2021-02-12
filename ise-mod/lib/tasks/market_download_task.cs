#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, client_bind.cs, Created 2021-02-11

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using Inventory;
using ise.components;
using ise.dialogs;
using ise_core.db;
using LiteDB;
using RestSharp;
using Verse;
using static ise_core.rest.Helpers;
using static ise.lib.Consts;
using static ise_core.rest.api.v1.consts;

namespace ise.lib.tasks
{
    internal class MarketDownloadDialogTask : AbstractDialogTask
    {
        private enum State
        {
            Start,
            Request,
            Caching,
            Done,
            Error,
        }

        private State state;
        private Task task;

        public MarketDownloadDialogTask(IDialog dialog) : base(dialog)
        {
            state = State.Start;
        }

        public override void Update()
        {
            // Handle task errors first.
            if (task != null && task.IsFaulted)
            {
                LogTaskError();
            }

            switch (state)
            {
                case State.Start:
                    Dialog.DialogMessage = "Connecting to server";
                    StartDownload();
                    break;
                case State.Request:
                    Dialog.DialogMessage = "Downloading Market Data";
                    if (task != null && task.IsCompleted)
                    {
                        ProcessInventoryReply(((Task<InventoryReply>) task).Result);
                    }

                    break;
                case State.Caching:
                    Dialog.DialogMessage = "Building Cache";
                    if (task != null && task.IsCompleted)
                    {
                        state = State.Done;
                    }

                    break;
                case State.Done:
                    Dialog.DialogMessage = "Market OK";
                    Done = true;
                    break;
                case State.Error:
                    Dialog.CloseDialog();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogTaskError()
        {
            state = State.Error;
            Logging.WriteErrorMessage($"Unhandled exception in task {task.Exception}");
            task = null;
        }

        private void StartDownload()
        {
            DBInventory inventory;
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(DBLocation))
            {
                inventory = db.GetCollection<DBInventory>().FindAll().FirstOrDefault();
            }

            if (inventory != null && inventory.InventoryPromiseExpires > GetUTCNow())
            {
                // Jump straight to done, cache is still valid
                state = State.Done;
            }
            else
            {
                var gc = Current.Game.GetComponent<ISEGameComponent>();
                var request = new InventoryRequest {ColonyId = gc.ColonyBind, ClientBindId = gc.ClientBind};
                Logging.WriteMessage($"Asking server for Inventory");
                task = SendAndParseReplyAsync(
                    request,
                    InventoryReply.Parser,
                    $"{URLPrefix}inventory/",
                    Method.POST,
                    gc.ClientBind
                );
                task.Start();
                state = State.Request;
            }
        }

        private void ProcessInventoryReply(InventoryReply reply)
        {
            Logging.WriteMessage($"Inventory Received, Promise {reply.InventoryPromiseId}");
            task = new Task(delegate
            {
                Logging.WriteMessage($"Building cache of {reply.Items.Count}");
                // Open database (or create if doesn't exist)
                using (var db = new LiteDatabase(DBLocation))
                {
                    var tradableCache = db.GetCollection<DBCachedTradable>();
                    tradableCache.DeleteAll();
                    tradableCache.InsertBulk(
                        reply.Items.Select(
                            tradable => new DBCachedTradable
                            {
                                ItemCode = tradable.ItemCode,
                                ThingDef = tradable.ThingDef,
                                Quantity = tradable.Quality,
                                Quality = tradable.Quantity,
                                Stuff = tradable.Stuff,
                                Weight = tradable.Weight,
                                WeBuyAt = tradable.WeBuyAt,
                                WeSellAt = tradable.WeSellAt,
                                Minified = tradable.Minified
                            }));
                    var inventoryCache = db.GetCollection<DBInventory>();
                    inventoryCache.DeleteAll();
                    inventoryCache.Insert(new DBInventory
                    {
                        InventoryPromiseId = reply.InventoryPromiseId,
                        InventoryPromiseExpires = reply.InventoryPromiseExpires,
                        CollectionChargePerKG = reply.CollectionChargePerKG,
                        DeliveryChargePerKG = reply.DeliveryChargePerKG, AccountBalance = reply.AccountBalance
                    });
                    Logging.WriteMessage($"Caching done");
                }
            });
            task.Start();

            // Go to next step
            state = State.Caching;
        }

        // private void ProcessConfirmBindReply(ConfirmBindReply reply)
        // {
        //     if (reply.IsValid)
        //     {
        //         if (reply.BindComplete)
        //         {
        //             switch (reply.BindType)
        //             {
        //                 case BindTypeEnum.AccountBind:
        //
        //
        //                     Logging.WriteMessage($"Account bind confirmed: new Client Id: {reply.ClientBindId}");
        //
        //                     // Now confirm the Client Bind ID
        //                     state = State.Caching;
        //                     var request = new ConfirmBindRequest()
        //                         {BindId = reply.ClientBindId, BindType = BindTypeEnum.ClientBind};
        //                     task = SendAndParseReplyAsync(
        //                         request,
        //                         ConfirmBindReply.Parser,
        //                         $"{URLPrefix}binder/bind_confirm",
        //                         Method.POST
        //                     );
        //                     task.Start();
        //                     return;
        //                 case BindTypeEnum.ClientBind:
        //                     Logging.WriteMessage($"Client bind confirmed: saving client Id: {reply.ClientBindId}");
        //                     var gc = Current.Game.GetComponent<ISEGameComponent>();
        //                     gc.ClientBind = reply.ClientBindId;
        //                     SaveBind<DBClientBind>(IseBootStrap.User.UserId, reply.ClientBindId);
        //                     StartBindVerify();
        //                     return;
        //                 default:
        //                     throw new ArgumentOutOfRangeException();
        //             }
        //         }
        //
        //         // Check if there's enough time left to go around again
        //         if (reply.TTL > 10)
        //         {
        //             // Sleep for 10 seconds;
        //             task = new Task(delegate { Thread.Sleep(10 * 1000); });
        //             state = State.WaitConfirm;
        //             Dialog.DialogMessage = $"Waiting for confirmation (About {reply.TTL}s left)";
        //             task.Start();
        //             return;
        //         }
        //     }
        //
        //     // Bind invalid, go back to start
        //     bindId = "";
        //     Logging.WriteMessage("Bind expired or was invalid, Requesting new bind");
        //     task = null;
        //     state = State.Start;
        // }
    }
}