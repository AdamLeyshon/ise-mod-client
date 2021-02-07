using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Bind;
using Colony;
using Hello;
using Inventory;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Order;
using RestSharp;
using Tradable;
using static ise_core.extend.IEnumHelpers;

namespace ise_core_tests
{
    [TestFixture]
    public class Tests
    {
        public const string TestClientBindID = "0IFEmQt8MWmmoK38qn";
        public const string TestClientSteamID = "[U:1:13457876]";
        public const string TestColonyID = "6Y1ebXmewlqaSCLmvCp7tpgWcdaEbRBN";


        [Test]
        public void Hello()
        {
            var helloRequest = new HelloRequest() {ClientVersion = "1.0.0"};
            var reply = ise_core.rest.Helpers.SendAndParseReply(helloRequest, HelloReply.Parser, "/api/v1/system/hello",
                Method.POST);
            Assert.IsNotEmpty(reply.ServerVersion);
        }

        [Test]
        public void BindVerify()
        {
            var request = new ClientBindVerifyRequest() {SteamId = TestClientSteamID, ClientBindId = TestClientBindID};
            var reply = ise_core.rest.Helpers.SendAndParseReply(request, ClientBindVerifyReply.Parser,
                "/api/v1/binder/bind_verify", Method.POST);
            Assert.IsTrue(reply.Valid);
        }


        [Test]
        public void UpdateColony()
        {
            var request = new ColonyUpdateRequest()
                {ClientBindId = TestClientBindID};

            request.Data = new ColonyData()
            {
                ColonyId = TestColonyID,
                Tick = 1000
            };

            var reply = ise_core.rest.Helpers.SendNoReply(request,
                $"/api/v1/colony/", Method.PATCH, TestClientBindID);
            Assert.IsTrue(reply.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void SetModList()
        {
            var request = new ColonyModsSetRequest()
                {ClientBindId = TestClientBindID, ColonyId = TestColonyID};
            request.ModName.AddRange(new[] {"A", "B", "C"});
            var reply = ise_core.rest.Helpers.SendNoReply(request,
                $"/api/v1/colony/mods", Method.POST, TestClientBindID);
            Assert.IsTrue(reply.StatusCode == HttpStatusCode.OK);
        }

        // [Test]
        // public void SetTradablesList()
        // {
        //     var request = new ColonyTradableSetRequest()
        //         {ClientBindId = TestClientBindID, ColonyId = TestColonyID};
        //
        //     var tradables = new[]
        //     {
        //         new ColonyTradable
        //         {
        //             ThingDef = "a",
        //             Quality = -1,
        //             Minified = false,
        //             BaseValue = 1,
        //             Stuff = ""
        //         },
        //         new ColonyTradable
        //         {
        //             ThingDef = "b",
        //             Quality = 1,
        //             Minified = false,
        //             BaseValue = 1,
        //             Stuff = ""
        //         },
        //         new ColonyTradable
        //         {
        //             ThingDef = "qq",
        //             Quality = 2,
        //             Minified = true,
        //             BaseValue = 1,
        //             Stuff = ""
        //         }
        //     };
        //
        //     request.Item.AddRange(tradables);
        //     var reply = ise_core.rest.Helpers.SendNoReply(request,
        //         $"/api/v1/colony/tradables", Method.POST, TestClientBindID);
        //     Assert.IsTrue(reply.StatusCode == HttpStatusCode.OK);
        // }

        [Test]
        public void GetInventory()
        {
            var request = new InventoryRequest()
                {ClientBindId = TestClientBindID, ColonyId = TestColonyID};

            var reply = ise_core.rest.Helpers.SendAndParseReply(request, InventoryReply.Parser,
                $"/api/v1/inventory/", Method.POST, TestClientBindID);
            var now = ise_core.rest.Helpers.GetUTCNow();
            Assert.Greater(reply.InventoryPromiseExpires, now);
            Assert.GreaterOrEqual(reply.InventoryPromiseExpires - now, 200);
            Assert.IsNotEmpty(reply.InventoryPromiseId);
        }

        [Test]
        public void TestPlaceOrder()
        {
            var inventoryRequest = new InventoryRequest()
                {ClientBindId = TestClientBindID, ColonyId = TestColonyID};

            var inventoryReply = ise_core.rest.Helpers.SendAndParseReply(inventoryRequest, InventoryReply.Parser,
                $"/api/v1/inventory/", Method.POST, TestClientBindID);

            Assert.NotZero(inventoryReply.Items.Count);
            
            var orderRequest = new OrderRequest()
            {
                ClientBindId = TestClientBindID, ColonyId = TestColonyID,
                InventoryPromiseId = inventoryReply.InventoryPromiseId, ColonyTick = 1001
            };

            orderRequest.WantToBuy.Add(new OrderItem()
                {ItemCode = inventoryReply.Items.Random().ItemCode, Quantity = 1, Health = 100});

            Thread.Sleep(1000);

            orderRequest.WantToSell.Add(new OrderItem()
                {ItemCode = inventoryReply.Items.Random().ItemCode, Quantity = 1, Health = 100});

            var reply = ise_core.rest.Helpers.SendAndParseReply(orderRequest, OrderReply.Parser,
                $"/api/v1/order/", Method.POST, TestClientBindID);
            var now = ise_core.rest.Helpers.GetUTCNow();
            Assert.AreNotEqual(Order.OrderRequestStatus.Rejected, reply.Status);
        }
    }
}