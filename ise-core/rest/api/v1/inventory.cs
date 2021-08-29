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

        public static InventoryReply GetInventory(string clientBindId, string colonyId)
        {
            var task = GetInventoryAsync(clientBindId, colonyId);
            task.Start();
            task.Wait();
            return task.Result;
        }

        public static Task<InventoryReply> GetInventoryAsync(string clientBindId, string colonyId)
        {
            var request = new InventoryRequest { ColonyId = colonyId, ClientBindId = clientBindId };
            return Helpers.SendAndParseReplyAsync(
                request,
                InventoryReply.Parser,
                $"{URLPrefix}inventory/",
                Method.POST,
                clientBindId
            );
        }

        #endregion
    }
}