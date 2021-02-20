#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, helpers.cs, Created 2020-10-01

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using RestSharp;
using System.Threading;
using static ise_core.rest.api.v1.Constants;

namespace ise_core.rest
{
    public static class Helpers
    {
        #region Methods

        public static long GetUTCNow()
        {
            return (long) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static RestClient CreateRestClient()
        {
            var client = new RestClient(Server)
            {
                AutomaticDecompression = true,
                UserAgent = $"ISE/1.0",
            };
            return client;
        }

        public static TR SendAndParseReply<TS, TR>(
            TS sendPacket,
            MessageParser<TR> parser,
            string url,
            Method method = Method.GET,
            string clientId = null,
            Dictionary<string, string> urlSegments = null
        )
            where TS : IMessage<TS>, new()
            where TR : IMessage<TR>, new()
        {
            var result = DoRequest(sendPacket, url, method, clientId, urlSegments);
            Console.WriteLine($"ProtoBuf Message size was {result.RawBytes.LongLength} bytes");
            return parser.ParseFrom(result.RawBytes);
        }

        public static Task<TR> SendAndParseReplyAsync<TS, TR>(
            TS sendPacket,
            MessageParser<TR> parser,
            string url,
            Method method = Method.GET,
            string clientId = null,
            Dictionary<string, string> urlSegments = null
        )
            where TS : IMessage<TS>, new()
            where TR : IMessage<TR>, new()
        {
            var task = new Task<TR>(delegate
            {
                var result = DoRequest(sendPacket, url, method, clientId, urlSegments);
                Console.WriteLine($"ProtoBuf Message size was {result.RawBytes.LongLength} bytes");
                return parser.ParseFrom(result.RawBytes);
            });
            return task;
        }

        public static IRestResponse SendNoReply<TS>(
            TS sendPacket,
            string url,
            Method method = Method.GET,
            string clientId = null,
            Dictionary<string, string> urlSegments = null
        )
            where TS : IMessage<TS>, new()
        {
            return DoRequest(sendPacket, url, method, clientId, urlSegments);
        }

        private static IRestResponse DoRequest<TS>(
            TS sendPacket,
            string url,
            Method method,
            string clientId,
            Dictionary<string, string> urlSegments
        ) where TS : IMessage<TS>, new()
        {
            Task<IRestResponse> task = null;
            for (var tries = 0; tries < 3; tries++)
            {
                task = MakeRequestAsync(
                    sendPacket,
                    url,
                    method,
                    clientId,
                    urlSegments
                );
                task.Wait();

                if (task.Result.IsSuccessful)
                {
                    return task.Result;
                }

                var tryAgain = task.Result.Headers.Any(p => p.Name?.ToLower() == "retry-after");

                // If we've reached max try count or server says does not ask to try again 
                // Then exit immediately
                if (!tryAgain) break;
                if (tries < 2)
                {
                    Thread.Sleep(5 * 1000); // Sleep for 5 seconds
                }
            }

            if (task == null)
                throw new Exception(
                    $"HTTP Client in Invalid State, task was null! wat do?");

            // Throw the task exception if there was one
            if (task.Exception != null)
            {
                throw task.Exception;
            }

            // Otherwise generate one from HTTP Response
            throw new Exception(
                $"{task.Result.ErrorMessage}, HTTP Code: {task.Result.StatusCode:D}, {task.Result.StatusDescription}");
        }


        private static IRestResponse DoRequestAsync<TS>(
            TS sendPacket,
            string url,
            Method method,
            string clientId,
            Action callback,
            Dictionary<string, string> urlSegments
        ) where TS : IMessage<TS>, new()
        {
            Task<IRestResponse> task = null;
            for (var tries = 0; tries < 3; tries++)
            {
                task = MakeRequestAsync(
                    sendPacket,
                    url,
                    method,
                    clientId,
                    urlSegments
                );
                task.Wait();

                if (task.Result.IsSuccessful)
                {
                    return task.Result;
                }

                var tryAgain = task.Result.Headers.Any(p => p.Name?.ToLower() == "retry-after");

                // If we've reached max try count or server says does not ask to try again 
                // Then exit immediately
                if (!tryAgain) break;
                if (tries < 2)
                {
                    Thread.Sleep(5 * 1000); // Sleep for 5 seconds
                }
            }

            if (task == null)
                throw new Exception(
                    $"HTTP Client in Invalid State, task was null! wat do?");

            // Throw the task exception if there was one
            if (task.Exception != null)
            {
                throw task.Exception;
            }

            // Otherwise generate one from HTTP Response
            throw new Exception(
                $"{task.Result.ErrorMessage}, HTTP Code: {task.Result.StatusCode:D}, {task.Result.StatusDescription}");
        }

        public static Task<IRestResponse> MakeRequestAsync<TS>(
            TS sendPacket,
            string url,
            Method method = Method.GET,
            string clientId = null,
            Dictionary<string, string> urlSegments = null
        )
            where TS : IMessage<TS>
        {
            var client = CreateRestClient();
            var buf = sendPacket.ToByteArray();
            var request = new RestRequest(url);

            if (buf.LongLength > 256)
            {
#if DEBUG
                Console.WriteLine($"Uncompressed payload size {buf.LongLength}");
#endif
                request.AddParameter("Content-Encoding", "gzip", ParameterType.HttpHeader);
                buf = CompressBuffer(buf);
#if DEBUG
                Console.WriteLine($"Compressed payload size {buf.LongLength}");
#endif
            }

            request.AddParameter("", buf, ParameterType.RequestBody);

            return ExecAsync(client, request, method, clientId, urlSegments);
        }

        public static Task<IRestResponse> ExecAsync(RestClient client, RestRequest request, Method method = Method.GET,
            string clientId = null,
            Dictionary<string, string> urlSegments = null)
        {
            request.AddParameter("Accept", "application/protobuf", ParameterType.HttpHeader);
            request.AddParameter("Content-Type", "application/protobuf", ParameterType.HttpHeader);

            if (clientId != null)
                request.AddParameter("x-ise-client-id", clientId, ParameterType.HttpHeader);

            if (urlSegments != null)
            {
                foreach (var kvp in urlSegments)
                {
                    request.AddParameter(kvp.Key, kvp.Value, ParameterType.UrlSegment);
                }
            }

            request.RequestFormat = DataFormat.None;
            request.AddDecompressionMethod(DecompressionMethods.GZip);

            // Make request.
            return client.ExecuteAsync(request, method);
        }

        public static byte[] CompressBuffer(byte[] input)
        {
            var buffer = new byte[4096];
            using (var outStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    using (var mStream = new MemoryStream(input))
                    {
                        int read;
                        while ((read = mStream.Read(buffer, 0, buffer.Length)) > 0) gZipStream.Write(buffer, 0, read);
                    }
                }

                return outStream.ToArray();
            }
        }

        #endregion
    }
}