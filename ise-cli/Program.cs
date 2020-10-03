using System;
using ise_core.rest;
using Helloworld;

namespace ise_cli
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var send = new HelloRequest() {Name = "Adam"};
                var client = Helpers.CreateRestClient();
                var reply = Helpers.SendAndReply<HelloReply, HelloRequest>(client, send, HelloReply.Parser,
                    "/api/v1/player/");
                Console.WriteLine(reply.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}