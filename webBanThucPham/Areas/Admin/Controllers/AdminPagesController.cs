using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Helper;
using webBanThucPham.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;
namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminPagesController : Controller
    {
        private readonly DbBanThucPhamContext _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment; 

        public AdminPagesController(DbBanThucPhamContext context, INotyfService notyf, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notyf = notyf;
            _webHostEnvironment = webHostEnvironment; 
        }
        // GET: Admin/AdminPages
        public async Task<IActionResult> Index(int? page)
        {
            ViewData["CurrentPage"] = "Quản lý trang"; // Tiêu đề trang

            int pageSize = 5; // Số trang hiển thị trên mỗi trang
            int pageNumber = page ?? 1; // Nếu không có tham số `page`, mặc định là trang 1

            var pages = await _context.Pages
                                      .OrderByDescending(p => p.CreateDate)
                                      .ToPagedListAsync(pageNumber, pageSize);

            return View(pages);
        }


        // GET: Admin/AdminPages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .FirstOrDefaultAsync(m => m.PageId == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }
        [HttpPost]
        public JsonResult UploadImage(IFormFile upload)
        {
            return Helper.UploadImage.Upload(upload); // Gọi đúng phương thức Upload
        }
        // GET: Admin/AdminPages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminPages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Admin/AdminPages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Page page, IFormFile? ThumbFile)
        {
            ViewData["SecondPage"] = "Danh sách trang";
            ViewData["SecondController"] = "AdminPages";
            ViewData["SecondAction"] = "Index";
            ViewData["CurrentPage"] = "Tạo trang";

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(page);
                }

                // Xử lý ảnh đại diện
                page.Thumb = ThumbFile != null ? await FileHelper.SaveFileAsync(ThumbFile) : "default-thumbnail.jpg";

                // Tạo Alias, Meta tự động
                // Nếu Alias trống, tự động tạo từ PageName
                page.Alias = string.IsNullOrWhiteSpace(page.Alias)
                             ? AliasHelper.GenerateAlias(page.PageName)
                             : page.Alias;
                page.MetaDesc = SeoHelper.GenerateMetaDescription(page.PageName, "", page.Contents ?? "");
                page.MetaKey = SeoHelper.GenerateMetaKeywords(page.PageName, "");


                // Lưu vào database
                _context.Add(page);
                await _context.SaveChangesAsync();

                _notyf.Success("🎉 Trang đã được tạo thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"❌ Lỗi: {ex.Message}");
                return View(page);
            }
        }
        // GET: Admin/AdminPages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        // POST: Admin/AdminPages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Page page, IFormFile? ThumbFile)
        {
            if (id != page.PageId)
            {
                _notyf.Error("Trang không tồn tại!");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _notyf.Warning("Vui lòng kiểm tra lại thông tin trang.");
                return View(page);
            }

            try
            {
                var existingPage = await _context.Pages.FindAsync(id);
                if (existingPage == null)
                {
                    _notyf.Error("Trang không tồn tại!");
                    return NotFound();
                }

                // Cập nhật thông tin trang
                existingPage.PageName = page.PageName;
                existingPage.Contents = page.Contents;
                existingPage.Published = page.Published;
                existingPage.Title = page.Title;
                existingPage.MetaDesc = page.MetaDesc;
                existingPage.MetaKey = page.MetaKey;
                existingPage.Alias = page.Alias;
                existingPage.Ordering = page.Ordering;
                existingPage.CreateDate = page.CreateDate;

                // Xử lý upload ảnh mới
                if (ThumbFile != null)
                {
                    Console.WriteLine($"Old Thumb: {existingPage.Thumb}");
                    FileHelper.DeleteFile(existingPage.Thumb); // Xóa ảnh cũ
                    existingPage.Thumb = await FileHelper.SaveFileAsync(ThumbFile); // Lưu ảnh mới
                    Console.WriteLine($"New Thumb: {existingPage.Thumb}");
                }

                // Cập nhật database
                _context.Update(existingPage);
                await _context.SaveChangesAsync();
                _notyf.Success("🎉 Cập nhật trang thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"❌ Lỗi: {ex.Message}");
                return View(page);
            }
        }

        // POST: Admin/AdminPages/Delete/5
        [HttpPost]  // Thay vì [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trang." });
            }

            try
            {
                _context.Pages.Remove(page);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private bool PageExists(int id)
        {
            return _context.Pages.Any(e => e.PageId == id);
        }
    }
}
