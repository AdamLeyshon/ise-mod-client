using System;
using System.Diagnostics.Eventing.Reader;
using Bind;
using Hello;
using ise_core.rest;
using ise_core.system;

namespace ise_cli
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Ready, Enter Command:");
                var command = Console.ReadLine();
                switch (command)
                {
                    case "hello":
                        HelloRequest();
                        break;
                    case "bind":
                        BindRequest();
                        break;
                    case "quit":
                        System.Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Not a valid command");
                        break;
                }
            }
        }

        private static void HelloRequest()
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

        private static void BindRequest()
        {
            try
            {
                Console.Write("Steam ID? Press Enter for default, type n for None or enter your own Steam ID");
                var command = Console.ReadLine();
                var steamId = "76561197973723604";
                switch (command)
                {
                    case "":
                        break;
                    case "n":
                        steamId = "";
                        break;
                    default:
                        steamId = command;
                        break;
                }
                
                var send = new BindRequest() {SteamId = steamId};
                var client = Helpers.CreateRestClient();
                var reply = Helpers.SendAndReply(client, send, BindReply.Parser, "/api/v1/client_binder/bind");
                Console.WriteLine(reply);
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.InnerException != null ? e.InnerException.ToString() : "No Inner Exception");
            }
        }
    }
}