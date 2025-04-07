using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using webBanThucPham.Models;
using X.PagedList;
using YourNamespace.Helper;
namespace webBanThucPham.Controllers
{
    public class BlogController : Controller
    {
        private readonly DbBanThucPhamContext _context;
        public BlogController(DbBanThucPhamContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? page, string search, string author, string catName)
        {
            int pageSize = 6; // Số bài viết trên mỗi trang
            int pageNumber = page ?? 1;

            var query = _context.Tintucs
                .Where(t => t.Published) // Chỉ lấy bài viết đã xuất bản
                .OrderByDescending(t => t.CreatedDate)
                .Include(t => t.Cat) // Load thông tin danh mục
                .AsQueryable();


            // Tìm kiếm theo tiêu đề
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n => EF.Functions.Like(n.Title ?? "", $"%{search}%"));
            }

            // Tìm kiếm theo tác giả
            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(n => EF.Functions.Like(n.Author ?? "", $"%{author}%"));
            }

            // Tìm kiếm theo danh mục
            if (!string.IsNullOrWhiteSpace(catName))
            {
                query = query.Where(n => n.Cat != null && EF.Functions.Like(n.Cat.CatName, $"%{catName}%"));
            }
            // ✅ Lấy danh sách danh mục và truyền vào ViewBag
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var newsList = await query.ToPagedListAsync(pageNumber, pageSize);


            //var newfeedPosts = query
            //    .Where(t => t.IsNewfeed) // Chỉ lấy bài viết được gắn là newfeed
            //    .OrderByDescending(t => t.CreatedDate)
            //    .Take(5)
            //    .ToList();
            //ViewBag.NewfeedPosts = newfeedPosts;
            var newfeedPosts = query
                        .OrderByDescending(t => t.CreatedDate)
                        .Take(5)
                        .ToList();


            // Giữ lại giá trị tìm kiếm khi phân trang
            ViewBag.Search = search;
            ViewBag.Author = author;
            ViewBag.CatName = catName;

            return View(newsList);
        }
        public async Task<IActionResult> Detail(int id, string searchQuery = "")
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // Lấy tin tức theo ID
            var news = await _context.Tintucs
                .Include(t => t.Cat) // Load danh mục tin tức
                .FirstOrDefaultAsync(t => t.PostId == id && t.Published);

            if (news == null)
            {
                return NotFound();
            }

            // Lấy 4 tin tức có lượt xem nhiều nhất (không bao gồm tin hiện tại)
            var topNews = await _context.Tintucs
                .Where(t => t.Published && t.PostId != id)
                .OrderByDescending(t => t.Views)
                .Take(4)
                .ToListAsync();

            // Lấy 4 bài viết liên quan
            var relatedNews = await TinTucLienQuan.GetRelatedNews(_context, news, 4);

            // Xử lý tìm kiếm bài viết theo tiêu đề hoặc tác giả
            var searchResults = new List<Tintuc>();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchResults = await _context.Tintucs
                    .Where(t => t.Published &&
                        (t.Title.Contains(searchQuery) || t.Author.Contains(searchQuery)))
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(5) // Giới hạn số kết quả hiển thị
                    .ToListAsync();
            }

            // Đẩy dữ liệu sang View
            ViewBag.TopNews = topNews;
            ViewBag.Tags = news.Tags;
            ViewBag.RelatedNews = relatedNews;
            ViewBag.SearchResults = searchResults;
            ViewBag.SearchQuery = searchQuery;
            // Cập nhật view của bài viết lên 
            news.Views+=1;
            _context.Update(news);
            await _context.SaveChangesAsync();
            return View(news);
        }

    }
}
