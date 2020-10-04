#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-core, helpers.cs, Created 2020-10-01

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using RestSharp;

namespace ise_core.rest
{
    public static class Helpers
    {
        public static RestClient CreateRestClient()
        {
            var client = new RestClient("http://127.0.0.1:8000")
            {
                AutomaticDecompression = true,
                UserAgent = $"ISE/1.0",
            };
            return client;
        }

        public static TR SendAndReply<TR, TS>(RestClient c, TS sendPacket,
            MessageParser<TR> unpacker, String url)
            where TS : IMessage<TS>, new()
            where TR : IMessage<TR>, new()
        {
            var task = SendAndExpectReplyAsync<TS>(CreateRestClient(), sendPacket, url);
            task.Wait();
            if (task.Result.IsSuccessful)
            {
                return unpacker.ParseFrom(task.Result.RawBytes);
            }
            else
            {
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
                else
                {
                    throw new Exception(
                        $"{task.Result.ErrorMessage}, HTTP Code: {task.Result.StatusCode}, {task.Result.StatusDescription}");
                }
            }
        }

        public static Task<IRestResponse> SendAndExpectReplyAsync<TS>(RestClient c, TS sendPacket, String url)
            where TS : IMessage<TS>
        {
            var client = CreateRestClient();
            var buf = sendPacket.ToByteArray();
            var request = new RestRequest(url, Method.POST);

            if (buf.LongLength > 256)
            {
                request.AddParameter("Content-Encoding", "gzip", ParameterType.HttpHeader);
                buf = CompressBuffer(buf);
            }

            request.AddParameter("", buf, ParameterType.RequestBody);
            request.AddParameter("Accept", "application/protobuf", ParameterType.HttpHeader);
            request.AddParameter("Content-Type", "application/protobuf", ParameterType.HttpHeader);

            request.RequestFormat = DataFormat.None;
            request.AddDecompressionMethod(DecompressionMethods.GZip);

            // Make request.
            return client.ExecutePostAsync(request);
        }

        public static byte[] CompressBuffer(byte[] input)
        {
            var buffer = new byte[4096];
            using var outStream = new MemoryStream();
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
}