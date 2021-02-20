#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, inventory.cs, Created 2021-02-09

#endregion

using Inventory;
using ise_core.rest;
using RestSharp;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public class Inventory
    {
        #region Methods

        public static InventoryReply GetInventory(string clientBindId, string colonyId)
        {
            var request = new InventoryRequest {ClientBindId = clientBindId, ColonyId = colonyId};

            return Helpers.SendAndParseReply(request, InventoryReply.Parser,
                $"{URLPrefix}inventory/", Method.POST, clientBindId);
        }

        #endregion
    }
}