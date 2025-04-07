using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace webBanThucPham.Helper
{
    public static class SeoHelper
    {
        /// <summary>
        /// Tạo Meta Description từ mô tả sản phẩm, tối đa 160 ký tự.
        /// </summary>
        public static string GenerateMetaDescription(string productName, string shortDesc, string description)
        {
            string metaDesc = !string.IsNullOrWhiteSpace(shortDesc) ? shortDesc : description;
            if (string.IsNullOrWhiteSpace(metaDesc))
            {
                metaDesc = $"Mua {productName} chất lượng cao, giá tốt tại cửa hàng.";
            }

            return metaDesc.Length > 160 ? metaDesc.Substring(0, 157) + "..." : metaDesc;
        }

        /// <summary>
        /// Tạo Meta Keywords từ tên sản phẩm và mô tả ngắn.
        /// </summary>
        public static string GenerateMetaKeywords(string productName, string shortDesc)
        {
            string content = $"{productName} {shortDesc}".ToLower();
            content = Regex.Replace(content, @"\s+", " "); // Xóa khoảng trắng dư thừa

            // Danh sách các từ không cần thiết
            string[] stopWords = { "của", "và", "cái", "một", "được", "cho", "với" };

            // Tạo danh sách từ khóa bằng cách loại bỏ các từ không cần thiết
            var keywords = content.Split(' ')
                                  .Where(word => !stopWords.Contains(word))
                                  .Distinct()
                                  .ToArray();

            return string.Join(", ", keywords);
        }
    }
}
