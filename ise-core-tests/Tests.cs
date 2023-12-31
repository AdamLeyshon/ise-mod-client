#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core-tests, Tests.cs 2021-07-09
// #endregion

#endregion

using System;
using System.Linq;
using Bind;
using Common;
using Hello;
using Inventory;
using ise_core.rest;
using NUnit.Framework;
using Order;
using RestSharp;
using Tradable;

namespace ise_core_tests
{
    [TestFixture]
    public class Tests
    {
        private const string TestClientBindID = "939eddf8-0a5f-42d9-b6b3-c2a8b9ebfc7d";
        private const string TestClientSteamID = "[U:1:13457876]";
        private const string TestColonyID = "e946511c-ab8f-4d3d-8ec4-6408ffe4ad1c";
        private const string TestOrderID = "df9d457e-1891-4c3e-a85a-42962ba572f2";


        [Test]
        public void TestHello()
        {
            var helloRequest = new HelloRequest { ClientVersion = "1.0.0" };
            var reply = Helpers.SendAndParseReply(helloRequest, HelloReply.Parser, "/api/v1/system/hello",
                Method.POST);
            Assert.IsNotEmpty(reply.ServerVersion);
        }

        [Test]
        public void TestBindVerify()
        {
            var request = new ClientBindVerifyRequest { SteamId = TestClientSteamID, ClientBindId = TestClientBindID };
            var reply = Helpers.SendAndParseReply(request, ClientBindVerifyReply.Parser,
                "/api/v1/binder/bind_verify", Method.POST);
            Assert.IsTrue(reply.Valid);
        }

        [Test]
        public void TestUpdateColony()
        {
            Assert.IsTrue(ise_core.rest.api.v1.Colony.UpdateColony(TestClientBindID, TestColonyID, 100));
        }

        [Test]
        public void TestSetModList()
        {
            Assert.IsTrue(
                ise_core.rest.api.v1.Colony.SetModList(TestClientBindID, TestColonyID, new[] { "A", "B", "C" }));
        }

        [Test]
        public void TestGetInventory()
        {
            var request = new InventoryRequest { ClientBindId = TestClientBindID, ColonyId = TestColonyID };

            var reply = Helpers.SendAndParseReply(request, InventoryReply.Parser,
                "/api/v1/inventory/", Method.POST, TestClientBindID);
            var now = Helpers.GetUTCNow();
            Assert.Greater(reply.InventoryPromiseExpires, now);
            Assert.GreaterOrEqual(reply.InventoryPromiseExpires - now, 200);
            Assert.IsNotEmpty(reply.InventoryPromiseId);
        }

        [Test]
        public void TestGetOrderList()
        {
            var request = new OrderListRequest { ClientBindId = TestClientBindID, ColonyId = TestColonyID };

            Helpers.SendAndParseReply(request, OrderListReply.Parser,
                "/api/v1/order/list", Method.POST, TestClientBindID);
        }

        [Test]
        public void TestGetOrderStatus()
        {
            var request = new OrderStatusRequest
                { ClientBindId = TestClientBindID, ColonyId = TestColonyID, OrderId = TestOrderID };

            var reply = Helpers.SendAndParseReply(request, OrderStatusReply.Parser,
                "/api/v1/order/", Method.POST, TestClientBindID);

            Assert.AreEqual(TestOrderID, reply.OrderId);
        }

        [Test]
        public void TestSetOrderStatusInvalid()
        {
            var request = new OrderUpdateRequest
            {
                ClientBindId = TestClientBindID, ColonyId = TestColonyID, OrderId = TestOrderID,
                Status = OrderStatusEnum.Placed
            };

            Assert.Catch<Exception>(delegate
            {
                Helpers.SendAndParseReply(request, OrderStatusReply.Parser,
                    "/api/v1/order/update", Method.POST, TestClientBindID);
            });
        }

        [Test]
        public void TestPlaceOrderThenTimeWarp()
        {
            // Set our Colony to Tick 100
            Assert.IsTrue(ise_core.rest.api.v1.Colony.UpdateColony(TestClientBindID, TestColonyID, 100));

            // Get our current bank balances
            var originalBankData = ise_core.rest.api.v1.Bank.GetBankData(TestClientBindID, TestColonyID).Reply;

            // Set our tradables
            var tradables = new[]
            {
                new ColonyTradable
                {
                    ThingDef = "UnitTestItem",
                    Quality = -1,
                    Minified = false,
                    BaseValue = 1,
                    Stuff = ""
                }
            };
            Assert.IsTrue(ise_core.rest.api.v1.Colony.SetTradablesList(TestClientBindID, TestColonyID, tradables));

            // Get inventory from server
            var inventoryReply = ise_core.rest.api.v1.Inventory.GetInventory(TestClientBindID, TestColonyID);
            Assert.NotZero(inventoryReply.Items.Count);
            var unitTestItem = inventoryReply.Items.First(x => x.ThingDef == "UnitTestItem");

            // Place the order
            var orderReply = ise_core.rest.api.v1.Order.PlaceOrder(TestClientBindID, TestColonyID,
                inventoryReply.InventoryPromiseId,
                100, null, new[]
                {
                    new OrderItem
                    {
                        ItemCode = unitTestItem.ItemCode,
                        Quantity = 1,
                        Health = 100
                    }
                });

            // Make sure the server accepted the order
            Assert.AreNotEqual(OrderRequestStatus.Rejected, orderReply.Status);
            Assert.AreEqual(OrderRequestStatus.AcceptedAll, orderReply.Status);

            // Get the current order status, should be delivered since we only sold something.
            Assert.AreEqual(OrderStatusEnum.Delivered,
                ise_core.rest.api.v1.Order.GetOrderStatus(
                    TestClientBindID,
                    TestColonyID,
                    orderReply.Data.OrderId
                ).Status
            );

            // Get new bank balance.
            var updatedBankData = ise_core.rest.api.v1.Bank.GetBankData(TestClientBindID, TestColonyID).Reply;

            // Check our balance went up by the value of the item
            Assert.AreEqual(
                originalBankData.Balance[(int)CurrencyEnum.Utc] + unitTestItem.BaseValue,
                updatedBankData.Balance[(int)CurrencyEnum.Utc]
            );

            // Go back in time to tick 99,
            // This will cause the above order to get rolled back.
            Assert.IsTrue(ise_core.rest.api.v1.Colony.UpdateColony(TestClientBindID, TestColonyID, 99));

            // Get bank balance after roll back.
            updatedBankData = ise_core.rest.api.v1.Bank.GetBankData(TestClientBindID, TestColonyID).Reply;

            // Check that our bank balance was reverted back to the original value.
            Assert.AreEqual(
                originalBankData.Balance[(int)CurrencyEnum.Utc],
                updatedBankData.Balance[(int)CurrencyEnum.Utc]
            );

            // Re-fetch the inventory
            inventoryReply = ise_core.rest.api.v1.Inventory.GetInventory(TestClientBindID, TestColonyID);
            var newUnitTestItem = inventoryReply.Items.First(x => x.ThingDef == "UnitTestItem");

            // Test that the stock has gone back to the original level.
            Assert.AreEqual(unitTestItem.Quantity, newUnitTestItem.Quantity);
        }
    }
}