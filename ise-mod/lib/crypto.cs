#region License

// This file was created by TwistedSoul @ TheCodeCache.net
// You are free to inspect the mod but may not modify or redistribute without my express permission.
// However! If you would like to contribute to GWP please feel free to drop me a message.
// 
// ise-mod, crypto.cs, Created 2021-02-10

#endregion

using System;
using System.Security.Cryptography;
using System.Text;

namespace ise.lib
{
    public static class Crypto
    {
        #region Methods

        internal static string GetShaHash(string input)
        {
            return BitConverter
                .ToString(
                    SHA1
                        .Create()
                        .ComputeHash(Encoding.UTF8.GetBytes(input))
                )
                .Replace("-", string.Empty);
        }

        #endregion
    }
}