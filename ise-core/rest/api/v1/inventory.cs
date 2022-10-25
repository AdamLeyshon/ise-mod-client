#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, inventory.cs 2021-02-09

#endregion

using System.Threading.Tasks;
using Inventory;
using RestSharp;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public class Inventory
    {
        #region Methods

        public static InventoryReply GetInventory(string clientBindId, string colonyId, bool continuePromise = false)
        {
            var task = GetInventoryAsync(clientBindId, colonyId, continuePromise);
            task.Start();
            task.Wait();
            return task.Result;
        }

        public static Task<InventoryReply> GetInventoryAsync(string clientBindId, string colonyId, bool continuePromise)
        {
            var request = new InventoryRequest { ColonyId = colonyId, ClientBindId = clientBindId, ContinueExistingPromise = continuePromise};
            return Helpers.SendAndParseReplyAsync(
                request,
                InventoryReply.Parser,
                $"{URLPrefix}inventory/",
                Method.POST,
                clientBindId
            );
        }
        
        public static Task<ActivatePromiseReply> ActivatePromiseAsync(string clientBindId, string colonyId, string promiseId)
        {
            var request = new ActivatePromiseRequest { ColonyId = colonyId, ClientBindId = clientBindId, InventoryPromiseId = promiseId};
            return Helpers.SendAndParseReplyAsync(
                request,
                ActivatePromiseReply.Parser,
                $"{URLPrefix}inventory/activate",
                Method.POST,
                clientBindId
            );
        }
        
        public static Task<GeneratePromiseReply> GeneratePromiseAsync(string clientBindId, string colonyId)
        {
            var request = new GeneratePromiseRequest { ColonyId = colonyId, ClientBindId = clientBindId};
            return Helpers.SendAndParseReplyAsync(
                request,
                GeneratePromiseReply.Parser,
                $"{URLPrefix}inventory/promise",
                Method.POST,
                clientBindId
            );
        }

        #endregion
    }
}