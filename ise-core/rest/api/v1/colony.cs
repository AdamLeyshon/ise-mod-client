#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, colony.cs, Created 2021-02-09

#endregion

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Colony;
using ise_core.rest;
using RestSharp;
using Tradable;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public static class Colony
    {
        #region Methods

        public static bool UpdateColony(string clientBindId, string colonyId, int tick)
        {
            var request = new ColonyUpdateRequest
            {
                ClientBindId = clientBindId, Data = new ColonyData {ColonyId = colonyId, Tick = tick}
            };

            var reply = Helpers.SendNoReply(request,
                $"{URLPrefix}colony/", Method.PATCH, clientBindId);
            return reply.StatusCode == HttpStatusCode.OK;
        }


        public static bool SetModList(string clientBindId, string colonyId, IEnumerable<string> modList)
        {
            var request = new ColonyModsSetRequest {ClientBindId = clientBindId, ColonyId = colonyId};
            request.ModName.AddRange(modList);
            var reply = Helpers.SendNoReply(request,
                $"{URLPrefix}colony/mods", Method.POST, clientBindId);
            return reply.StatusCode == HttpStatusCode.OK;
        }

        public static bool SetTradablesList(string clientBindId, string colonyId, IEnumerable<ColonyTradable> tradables)
        {
            var request = new ColonyTradableSetRequest()
                {ClientBindId = clientBindId, ColonyId = colonyId};

            request.Item.AddRange(tradables);
            return Helpers.SendNoReply(
                request,
                $"/api/v1/colony/tradables",
                Method.POST,
                clientBindId).IsSuccessful;
        }

        #endregion
    }
}