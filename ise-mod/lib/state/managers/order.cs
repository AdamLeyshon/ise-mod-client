#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, order.cs, Created 2021-02-26

#endregion

using System;
using ise_core.db;
using static ise.lib.Constants;
using LiteDB;
using Order;
using Steamworks;
using Verse;
using static RimWorld.GenDate;

namespace ise.lib.state.managers
{
    public class Order
    {
        private const int TicksPerHalfDay = TicksPerDay / 2;

        private readonly string orderId;
        private DBOrder backingOrder;
        internal OrderStatusEnum Status { get; private set; }
        internal bool Busy { get; private set; }

        public Order(string orderId)
        {
            this.orderId = orderId;
            using (var db = new LiteDatabase(DBLocation))
            {
                backingOrder = db.GetCollection<DBOrder>(Tables.Orders).FindById(this.orderId);
                Status = backingOrder.Status;
            }
        }

        public void Update()
        {
            Busy = true;
            Status = backingOrder.Status;
            var currentTick = Current.Game.tickManager.TicksGame;
            if (backingOrder.DeliveryTick - currentTick < TicksPerHalfDay)
            {
                // State check
                switch (backingOrder.Status)
                {
                    case OrderStatusEnum.Delivered:
                    case OrderStatusEnum.Failed:
                    case OrderStatusEnum.Reversed:
                        break;
                    case OrderStatusEnum.Placed:
                        ProcessStatePlaced(currentTick);
                        break;
                    case OrderStatusEnum.OutForDelivery:
                        ProcessStateOutForDelivery(currentTick);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Busy = false;
        }

        private void ProcessStatePlaced(int currentTick)
        {
        }

        private void ProcessStateOutForDelivery(int currentTick)
        {
            // Don't do anything until we reach the delivery time.
            if (currentTick < backingOrder.DeliveryTick) return;
            
            // Do something
            DeliverOrder();
        }
        
        private void DeliverOrder(){}
    }
}