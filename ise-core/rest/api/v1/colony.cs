#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, colony.cs 2021-02-09
// #endregion

#endregion

using System.Collections.Generic;
using System.Net;
using Colony;
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
                ClientBindId = clientBindId, Data = new ColonyData { ColonyId = colonyId, Tick = tick }
            };

            var reply = Helpers.SendNoReply(request,
                $"{URLPrefix}colony/", Method.PATCH, clientBindId);
            return reply.StatusCode == HttpStatusCode.OK;
        }


        public static bool SetModList(string clientBindId, string colonyId, IEnumerable<string> modList)
        {
            var request = new ColonyModsSetRequest { ClientBindId = clientBindId, ColonyId = colonyId };
            request.ModName.AddRange(modList);
            var reply = Helpers.SendNoReply(request,
                $"{URLPrefix}colony/mods", Method.POST, clientBindId);
            return reply.StatusCode == HttpStatusCode.OK;
        }

        public static bool SetTradablesList(string clientBindId, string colonyId, IEnumerable<ColonyTradable> tradables, bool firstBatch = true)
        {
            var request = new ColonyTradableSetRequest { ClientBindId = clientBindId, ColonyId = colonyId };

            request.Item.AddRange(tradables);
            return Helpers.SendNoReply(
                request,
                "/api/v1/colony/tradables",
                firstBatch ? Method.POST : Method.PATCH,
                clientBindId).IsSuccessful;
        }

        #endregion
    }
}