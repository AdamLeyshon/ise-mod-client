#region license

// #region License
// // This file was created by TwistedSoul @ TheCodeCache.net
// // You are free to inspect the mod but may not modify or redistribute without my express permission.
// // However! If you would like to contribute to this code please feel free to drop me a message.
// //
// // iseworld, ise-core, bank.cs 2021-05-18
// #endregion

#endregion

using Bank;
using RestSharp;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest.api.v1
{
    public class Bank
    {
        public static Helpers.TaskOrReply<BankDataReply> GetBankData(
            string clientBindId,
            string colonyId,
            bool returnAsync = false)
        {
            var request = new BankGetRequest { ColonyId = colonyId, ClientBindId = clientBindId };

            var task = Helpers.SendAndParseReplyAsync(
                request,
                BankDataReply.Parser,
                $"{URLPrefix}bank/",
                Method.POST,
                clientBindId
            );

            if (returnAsync) return new Helpers.TaskOrReply<BankDataReply> { Task = task };

            task.Start();
            task.Wait();
            return new Helpers.TaskOrReply<BankDataReply> { Reply = task.Result };
        }
    }
}