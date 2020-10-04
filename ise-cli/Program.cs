using System;
using ise_core.rest;
using Hello;
using ise_core.system;
namespace ise_cli
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var send = new HelloRequest() {ClientVersion = Consts.CLIENT_VERSION};
                var client = Helpers.CreateRestClient();
                var reply = Helpers.SendAndReply<HelloReply, HelloRequest>(client, send, HelloReply.Parser,
                    "/api/v1/system/hello");
                Console.WriteLine(reply.ServerVersion);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.InnerException != null ? e.InnerException.ToString() : "No Inner Exception");
            }
        }
    }
}