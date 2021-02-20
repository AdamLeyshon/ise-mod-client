#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, verify.cs, Created 2021-02-09

#endregion

using Bind;
using RestSharp;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1.bind
{
    public static class Verify
    {
        #region Methods

        internal static bool BindVerify(string steamId, string clientBindId)
        {
            var request = new ClientBindVerifyRequest {SteamId = steamId, ClientBindId = clientBindId};
            var reply = Helpers.SendAndParseReply(request, ClientBindVerifyReply.Parser,
                $"{URLPrefix}binder/bind_verify", Method.POST);
            return reply.Valid;
        }

        #endregion
    }
}