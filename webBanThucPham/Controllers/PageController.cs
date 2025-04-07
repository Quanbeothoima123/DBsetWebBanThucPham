using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;

namespace webBanThucPham.Controllers
{
    public class PageController : Controller
    {
        private readonly DbBanThucPhamContext _context;

        public PageController(DbBanThucPhamContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? id)
        {
            // Nếu không có id được truyền vào, mặc định hiển thị PageId = 4
            int pageId = id ?? 4;

            // Lấy bài viết theo ID
            var page = _context.Pages.FirstOrDefault(p => p.PageId == pageId);

            if (page == null)
            {
                return View("NotFound"); // Hiển thị trang lỗi 404 nếu không tìm thấy
            }

            // Lấy danh sách các trang khác (không trùng với trang hiện tại)
            var pages = _context.Pages
                .Where(p => p.PageId != pageId)
                .OrderByDescending(p => p.CreateDate)
                .ToList();

            ViewBag.Pages = pages; // Gửi danh sách page vào ViewBag để dùng trong sidebar

            return View(page);
        }
    }
}
