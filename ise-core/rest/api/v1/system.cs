#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, System.cs, Created 2021-02-03

#endregion

using Hello;
using RestSharp;

namespace ise_core.rest.api.v1
{
    public static class System
    {
        public static string Hello()
        {
            var helloRequest = new HelloRequest() {ClientVersion = "1.0.0"};
            return ise_core.rest.Helpers.SendAndParseReply(
                helloRequest, HelloReply.Parser, "/api/v1/system/hello",
                Method.POST
            ).ServerVersion;
        }
    }
}