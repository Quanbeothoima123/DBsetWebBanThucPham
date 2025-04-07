using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;

namespace YourNamespace.Helper
{
    public static class TinTucLienQuan
    {
        public static async Task<List<Tintuc>> GetRelatedNews(DbBanThucPhamContext context, Tintuc currentNews, int limit = 4)
        {
            return await context.Tintucs
                .Where(t => t.Published && t.PostId != currentNews.PostId)
                .OrderByDescending(t => t.Title.Contains(currentNews.Title) ? 1 : 0) // Ưu tiên bài có tiêu đề giống
                .ThenByDescending(t => t.CatId == currentNews.CatId ? 1 : 0) // Tiếp theo là cùng danh mục
                .ThenByDescending(t => t.Author == currentNews.Author ? 1 : 0) // Cuối cùng là cùng tác giả
                .Take(limit)
                .ToListAsync();
        }
    }
}
