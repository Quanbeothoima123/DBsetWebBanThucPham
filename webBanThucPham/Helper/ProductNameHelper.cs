using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace webBanThucPham.Helper
{
    public static class ProductNameHelper
    {
        private static readonly HashSet<string> BadWords = new HashSet<string>
        {
            "bậy1", "bậy2", "bậy3" // Thêm các từ cần lọc
        };

        /// <summary>
        /// Chuẩn hóa tên sản phẩm: loại bỏ ký tự đặc biệt, dấu cách dư, kiểm tra từ bậy bạ.
        /// </summary>
        public static string NormalizeProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return string.Empty;

            // Loại bỏ ký tự đặc biệt (chỉ giữ lại chữ cái, số, khoảng trắng)
            string cleanedName = Regex.Replace(productName, @"[^a-zA-ZÀ-ỹ0-9\s]", "").Trim();

            // Xóa khoảng trắng dư thừa
            cleanedName = Regex.Replace(cleanedName, @"\s+", " ");

            // Kiểm tra từ bậy bạ
            foreach (var badWord in BadWords)
            {
                if (cleanedName.IndexOf(badWord, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Tên sản phẩm không hợp lệ";
                }
            }

            return cleanedName;
        }
    }
}
