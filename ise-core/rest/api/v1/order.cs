#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, order.cs, Created 2021-02-09

#endregion

using System.Collections.Generic;
using ise_core.rest;
using Order;
using RestSharp;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public static class Order
    {
        #region Methods

        public static OrderListReply GetOrderList(string clientBindId, string colonyId)
        {
            var request = new OrderListRequest {ClientBindId = clientBindId, ColonyId = colonyId};

            return Helpers.SendAndParseReply(request, OrderListReply.Parser,
                $"{URLPrefix}order/list", Method.POST, clientBindId);
        }

        public static OrderStatusReply GetOrderStatus(string clientBindId, string colonyId, string orderId)
        {
            var request = new OrderStatusRequest {ClientBindId = clientBindId, ColonyId = colonyId, OrderId = orderId};

            return Helpers.SendAndParseReply(request, OrderStatusReply.Parser,
                $"{URLPrefix}order/status", Method.POST, clientBindId);
        }

        public static OrderStatusReply SetOrderStatus(string clientBindId, string colonyId, string orderId)
        {
            var request = new OrderUpdateRequest
            {
                ClientBindId = clientBindId, ColonyId = colonyId, OrderId = orderId,
                Status = OrderStatusEnum.OutForDelivery
            };

            return Helpers.SendAndParseReply(request, OrderStatusReply.Parser,
                $"{URLPrefix}order/update", Method.POST, clientBindId);
        }

        public static OrderReply PlaceOrder(
            string clientBindId,
            string colonyId,
            string promise,
            int tick,
            IEnumerable<OrderItem> wantToBuy,
            IEnumerable<OrderItem> wantToSell)
        {
            var orderRequest = new OrderRequest
            {
                ClientBindId = clientBindId, ColonyId = colonyId,
                InventoryPromiseId = promise, ColonyTick = tick,
            };

            orderRequest.WantToBuy.AddRange(wantToBuy);
            orderRequest.WantToSell.AddRange(wantToSell);

            return Helpers.SendAndParseReply(orderRequest, OrderReply.Parser,
                $"{URLPrefix}order/", Method.POST, clientBindId);
        }

        #endregion
    }
}