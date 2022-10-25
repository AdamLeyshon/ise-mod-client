#region license
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, contracts.cs 2022-07-12
#endregion

using System.Threading.Tasks;
using Inventory;
using RestSharp;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public class contracts
    {
        #region Methods

        public static InventoryReply GetContracts(string clientBindId, string colonyId, bool buy, bool sell)
        {
            var task = GetContractsAsync(clientBindId, colonyId);
            task.Start();
            task.Wait();
            return task.Result;
        }

        public static Task<InventoryReply> GetContractsAsync(string clientBindId, string colonyId)
        {
            var request = new InventoryRequest { ColonyId = colonyId, ClientBindId = clientBindId };
            return Helpers.SendAndParseReplyAsync(
                request,
                InventoryReply.Parser,
                $"{URLPrefix}contracts/",
                Method.POST,
                clientBindId
            );
        }
        
        // public static Task<ActivatePromiseReply> CreateContractAsync(string clientBindId, string colonyId)
        // {
        //     var request = new ActivatePromiseRequest { ColonyId = colonyId, ClientBindId = clientBindId, InventoryPromiseId = promiseId};
        //     return Helpers.SendAndParseReplyAsync(
        //         request,
        //         ActivatePromiseReply.Parser,
        //         $"{URLPrefix}contracts/",
        //         Method.POST,
        //         clientBindId
        //     );
        // }
        //
        #endregion
    }
}