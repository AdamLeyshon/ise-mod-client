#region license

// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, order.cs 2021-02-09

#endregion

using System.Collections.Generic;
using Common;
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
            var request = new OrderListRequest { ClientBindId = clientBindId, ColonyId = colonyId };

            return Helpers.SendAndParseReply(request, OrderListReply.Parser,
                $"{URLPrefix}order/list", Method.POST, clientBindId);
        }

        public static OrderStatusReply GetOrderStatus(string clientBindId, string colonyId, string orderId)
        {
            var request = new OrderStatusRequest
                { ClientBindId = clientBindId, ColonyId = colonyId, OrderId = orderId };

            return Helpers.SendAndParseReply(request, OrderStatusReply.Parser,
                $"{URLPrefix}order/", Method.POST, clientBindId);
        }

        public static OrderManifestReply GetOrderManifest(string clientBindId, string colonyId, string orderId)
        {
            var request = new OrderManifestRequest
            {
                ColonyId = colonyId,
                ClientBindId = clientBindId,
                OrderId = orderId
            };

            return Helpers.SendAndParseReply(
                request,
                OrderManifestReply.Parser,
                $"{URLPrefix}order/manifest",
                Method.POST,
                request.ClientBindId
            );
        }

        public static OrderStatusReply SetOrderStatus(OrderStatusEnum status, string clientBindId, string colonyId,
            string orderId, int tick)
        {
            var request = new OrderUpdateRequest
            {
                ClientBindId = clientBindId, ColonyId = colonyId, OrderId = orderId,
                Status = status,
                ColonyTick = tick
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
            IEnumerable<OrderItem> wantToSell,
            CurrencyEnum currency = CurrencyEnum.Utc,
            int additionalFunds = 0)
        {
            var orderRequest = new OrderRequest
            {
                ClientBindId = clientBindId,
                ColonyId = colonyId,
                InventoryPromiseId = promise,
                ColonyTick = tick,
                Currency = currency,
                AdditionalFunds = additionalFunds
            };

            if (wantToBuy != null) orderRequest.WantToBuy.AddRange(wantToBuy);

            if (wantToSell != null) orderRequest.WantToSell.AddRange(wantToSell);

            return Helpers.SendAndParseReply(orderRequest, OrderReply.Parser,
                $"{URLPrefix}order/place", Method.POST, clientBindId);
        }

        #endregion
    }
}