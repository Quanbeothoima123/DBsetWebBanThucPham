using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace webBanThucPham.Helper
{
    public static class AliasHelper
    {
        /// <summary>
        /// Tạo alias tự động từ tên sản phẩm (loại bỏ dấu, thay khoảng trắng bằng `-`).
        /// </summary>
        public static string GenerateAlias(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return string.Empty;

            // Chuyển thành chữ thường
            string alias = productName.ToLowerInvariant();

            // Loại bỏ dấu tiếng Việt
            alias = RemoveVietnameseAccents(alias);

            // Thay khoảng trắng bằng dấu "-"
            alias = Regex.Replace(alias, @"\s+", "-");

            // Loại bỏ ký tự không mong muốn (chỉ giữ lại chữ cái, số, dấu "-")
            alias = Regex.Replace(alias, @"[^a-z0-9-]", "");

            return alias;
        }

        /// <summary>
        /// Loại bỏ dấu tiếng Việt trong chuỗi.
        /// </summary>
        private static string RemoveVietnameseAccents(string text)
        {
            text = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
