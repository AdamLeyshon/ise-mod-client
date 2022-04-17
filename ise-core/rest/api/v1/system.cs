#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, system.cs 2021-02-09

#endregion

using System;
using System.Net;
using Hello;
using ise_core.system;
using RestSharp;
using static ise_core.rest.Helpers;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public static class System
    {
        #region Methods

        public static HandshakeResponse Handshake()
        {
            var send = new HelloRequest { ClientVersion = Consts.ClientVersion };
            var result = DoRequest(send, $"{URLPrefix}system/hello", Method.POST, null, null, false);
            Console.WriteLine($"ProtoBuf Message size was {result.RawBytes.LongLength} bytes");

            switch (result.StatusCode)
            {
                case HttpStatusCode.OK:
                    var reply = HelloReply.Parser.ParseFrom(result.RawBytes);
                    return new HandshakeResponse(reply.ServerVersion, HandshakeResult.OK);
                case HttpStatusCode.NotAcceptable:
                    return new HandshakeResponse("", HandshakeResult.ClientOutdated);
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.Gone:
                case HttpStatusCode.ServiceUnavailable:
                    return new HandshakeResponse("", HandshakeResult.UnderMaintenance);
            }

            return new HandshakeResponse("", HandshakeResult.Unreachable);
        }

        #endregion
    }

    public struct HandshakeResponse
    {
        public string ServerVersion;
        public HandshakeResult ResultCode;

        public HandshakeResponse(string serverVersion, HandshakeResult resultCode)
        {
            ServerVersion = serverVersion;
            ResultCode = resultCode;
        }
    }

    public enum HandshakeResult
    {
        Unreachable,
        ClientOutdated,
        UnderMaintenance,
        OK
    }
}