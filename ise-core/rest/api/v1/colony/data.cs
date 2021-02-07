#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, Data.cs, Created 2021-02-03

#endregion

using System;
using System.Collections.Generic;
using System.Net;
using Colony;
using RestSharp;
using Tradable;

namespace ise_core.rest.api.v1.colony
{
    public static class Data
    {
        public static void SetModList(IEnumerable<string> mods, string client_id, string colony_id)
        {
            var request = new ColonyModsSetRequest()
            {
                ClientBindId = client_id,
                ColonyId = colony_id
            };
            request.ModName.AddRange(mods);
            if (
                Helpers.SendNoReply(
                    request,
                    $"/api/v1/colony/mods",
                    Method.POST,
                    client_id
                ).StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("API did not accept request");
            }
        }

        public static void SetTradablesList(IEnumerable<ColonyTradable> tradables, string client_id, string colony_id)
        {
            var request = new ColonyTradableSetRequest()
                {ClientBindId = client_id, ColonyId = colony_id};

            request.Item.AddRange(tradables);
            if (Helpers.SendNoReply(
                request,
                $"/api/v1/colony/tradables",
                Method.POST,
                client_id).StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("API did not accept request");
            }
        }
    }
}