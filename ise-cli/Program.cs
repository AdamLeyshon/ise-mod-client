#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-cli, Program.cs 2021-07-09
// #endregion

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bank;
using Bind;
using Colony;
using Common;
using Google.Protobuf;
using Hello;
using ise_core.rest;
using ise_core.system;
using ObjectPrinter;
using RestSharp;

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
                    case "bindconfirm":
                        BindConfirm();
                        break;
                    case "bindverify":
                        BindVerify();
                        break;
                    case "colonycreate":
                        ColonyCreate();
                        break;
                    case "colonyget":
                        ColonyGet();
                        break;
                    case "colonyupdate":
                        ColonyUpdate();
                        break;
                    case "bankwithdraw":
                        BankWithdraw();
                        break;
                    case "quit":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Not a valid command");
                        break;
                }
            }
        }

        private static replyType RestWrapperSendAndReply<sendType, replyType>(sendType sendPacket,
            MessageParser<replyType> parser,
            string url,
            Method method = Method.GET,
            string clientId = null,
            Dictionary<string, string> urlSegments = null
        )
            where sendType : IMessage<sendType>, new()
            where replyType : IMessage<replyType>, new()
        {
            try
            {
                Console.WriteLine(">>>>>>>>>>>>>> SENDING >>>>>>>>>>>>>>");
                Console.WriteLine(sendPacket.DumpToString());
                Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var replyPacket = Helpers.SendAndParseReply(sendPacket, parser,
                    url, method, clientId, urlSegments);
                stopWatch.Stop();
                Console.WriteLine("");
                Console.WriteLine($"Total request time: {stopWatch.Elapsed:g} ");
                Console.WriteLine("");
                Console.WriteLine("<<<<<<<<<<<<< RECEIVED <<<<<<<<<<<<<<");
                Console.WriteLine(replyPacket.DumpToString());
                Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                return replyPacket;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.InnerException != null ? e.InnerException.ToString() : "No Inner Exception");
                throw;
            }
        }


        private static void HelloRequest()
        {
            var send = new HelloRequest { ClientVersion = Consts.ClientVersion };
            try
            {
                RestWrapperSendAndReply(send, HelloReply.Parser, "/api/v1/system/hello", Method.POST);
            }
            catch (Exception)
            {
            }
        }

        private static void BindRequest()
        {
            Console.Write("Enter Steam ID, Press Enter for default, type n for None or enter your own Steam ID");
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

            var send = new BindRequest { SteamId = steamId };
            try
            {
                RestWrapperSendAndReply(send, BindReply.Parser, "/api/v1/binder/bind", Method.POST);
            }
            catch (Exception)
            {
            }
        }

        private static void BindConfirm()
        {
            Console.Write("Enter Bind type, a or c:");
            var command = Console.ReadLine();
            BindTypeEnum bindType;
            switch (command)
            {
                case "a":
                    bindType = BindTypeEnum.AccountBind;
                    break;
                case "c":
                    bindType = BindTypeEnum.ClientBind;
                    break;
                default:
                    Console.WriteLine("Invalid type.");
                    return;
            }

            Console.Write("Enter Bind ID:");
            var bindID = Console.ReadLine();

            var send = new ConfirmBindRequest { BindType = bindType, BindId = bindID };

            try
            {
                RestWrapperSendAndReply(send, ConfirmBindReply.Parser, "/api/v1/binder/bind_confirm", Method.POST);
            }
            catch (Exception)
            {
            }
        }

        private static void BindVerify()
        {
            Console.Write("Enter Client Bind Id:");
            var bind = Console.ReadLine();

            Console.Write("Enter Steam Id:");
            var steam = Console.ReadLine();

            var send = new ClientBindVerifyRequest { SteamId = steam, ClientBindId = bind };
            try
            {
                RestWrapperSendAndReply(send, ClientBindVerifyReply.Parser, "/api/v1/binder/bind_verify", Method.POST);
            }
            catch (Exception)
            {
            }
        }

        private static void ColonyCreate()
        {
            Console.Write("Enter Client Bind ID:");
            var clientBindId = Console.ReadLine();
            var send = new ColonyCreateRequest
            {
                ClientBindId = clientBindId,
                Data = new ColonyData
                {
                    Name = "Test",
                    FactionName = "FactionTest",
                    MapId = 100,
                    Tick = 2222,
                    UsedDevMode = false,
                    GameVersion = "1.0.0",
                    Platform = PlatformEnum.Windows
                }
            };

            try
            {
                RestWrapperSendAndReply(send, ColonyData.Parser, "/api/v1/colony/", Method.POST, clientBindId);
            }
            catch (Exception)
            {
            }
        }

        private static void ColonyGet()
        {
            Console.Write("Enter Colony ID:");
            var colonyId = Console.ReadLine();

            Console.Write("Enter Client ID:");
            var clientId = Console.ReadLine();

            try
            {
                Helpers.SendAndParseReply(new ColonyGetRequest
                    {
                        ClientBindId = clientId,
                        ColonyId = colonyId
                    },
                    ColonyData.Parser,
                    $"/api/v1/colony/{colonyId}",
                    Method.GET,
                    clientId);
            }
            catch (Exception)
            {
            }
        }

        private static void ColonyUpdate()
        {
            var bind = "939eddf8-0a5f-42d9-b6b3-c2a8b9ebfc7d";
            var colonyId = "e946511c-ab8f-4d3d-8ec4-6408ffe4ad1c";
            var send = new ColonyCreateRequest
            {
                ClientBindId = bind,
                Data = new ColonyData
                {
                    ColonyId = colonyId,
                    Name = "Test1",
                    FactionName = "FactionTest",
                    MapId = 100,
                    Tick = 2222,
                    UsedDevMode = false,
                    GameVersion = "1.0.0"
                }
            };

            try
            {
                RestWrapperSendAndReply(send, ColonyData.Parser, $"/api/v1/colony/{colonyId}", Method.PATCH, bind);
            }
            catch (Exception)
            {
            }
        }

        private static void BankWithdraw()
        {
            var bind = "939eddf8-0a5f-42d9-b6b3-c2a8b9ebfc7d";
            var colonyId = "e946511c-ab8f-4d3d-8ec4-6408ffe4ad1c";
            var send = new BankWithdrawRequest
            {
                ClientBindId = bind,
                ColonyId = colonyId,
                Amount = 100,
                Currency = CurrencyEnum.Utc
            };

            try
            {
                RestWrapperSendAndReply(send, BankWithdrawReply.Parser, "/api/v1/bank/withdraw", Method.POST, bind);
            }
            catch (Exception)
            {
            }
        }
    }
}