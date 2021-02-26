#region License
// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, account.cs, Created 2021-02-26
#endregion

using System.Collections.Generic;
using System.Linq;
using ise_core.db;
using LiteDB;
using Order;
using Verse;
using static ise.lib.Constants;

namespace ise.lib.state.managers
{
    public class Account
    {
        private readonly Dictionary<string, Order> activeOrders;
        private readonly string accountId;
        private int nextUpdate;
        
        public Account(string colonyBindId)
        {
            accountId = colonyBindId;
            activeOrders = new Dictionary<string, Order>();
            nextUpdate = 0; // Update on next rare tick call.
            
            // Delete old orders from DB, new list will be pulled on next update.
            flush_orders();
        }

        internal void flush_orders()
        {
            using (var db = new LiteDatabase(DBLocation))
            {
                db.GetCollection<DBOrder>(Constants.Tables.Orders).DeleteMany(o => o.ColonyId == this.accountId);
            }
        }
        
        internal void add_order(string orderId)
        {
            using (var db = new LiteDatabase(DBLocation))
            {
                if (db.GetCollection<DBOrder>(Constants.Tables.Orders).FindById(orderId) == null)
                {
                    throw new KeyNotFoundException($"Unable to load order {orderId} from Database");
                }
            }
            activeOrders.Add(orderId, new Order(orderId));
        }

        internal void Update()
        {
            var currentTick = Current.Game.tickManager.TicksGame;
            if (currentTick < nextUpdate) return;
            nextUpdate = currentTick + OrderUpdateTickRate;
            foreach (var order in activeOrders.Values.Where(order => !order.Busy))
            {
                order.Update();
            }
        }
        
    }
}