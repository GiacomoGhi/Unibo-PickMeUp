using System;
using System.Security.Cryptography;
using System.Text;

namespace PickMeUp.Core.Common.Helpers;

public static class CryptographyHelper
{
    /// <summary>
    /// Provides a SHA 256 hash of the given string.
    /// </summary>
    public static string Hash(string value)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
