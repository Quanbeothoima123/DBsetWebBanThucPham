using System;
using System.Security.Cryptography;

namespace webBanThucPham.ExtensionCode
{
    public static class SecurityHelper
    {
        public static string GetRandomKey(int length = 16)
        {
            byte[] saltBytes = new byte[length];
            RandomNumberGenerator.Fill(saltBytes); // Thay thế RNGCryptoServiceProvider
            return Convert.ToBase64String(saltBytes); // Trả về chuỗi Base64
        }
    }
}
