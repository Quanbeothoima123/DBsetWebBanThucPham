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

namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminTintucsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public AdminTintucsController(INotyfService notyf, DbBanThucPhamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _notyf = notyf;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/AdminTintucs
        public async Task<IActionResult> Index(string searchTitle, bool? searchPublished, string searchAuthor, int? page)
        {
            int pageSize = 10; // Số lượng tin tức hiển thị trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là 1

            var tinTucs = _context.Tintucs
                .Include(t => t.Account)
                .Include(t => t.Cat)
                .OrderByDescending(t => t.CreatedDate)
                .AsQueryable(); // Chuyển sang IQueryable để có thể lọc dữ liệu

            // Lọc theo tiêu đề
            if (!string.IsNullOrEmpty(searchTitle))
            {
                tinTucs = tinTucs.Where(t => t.Title.Contains(searchTitle));
            }

            // Lọc theo trạng thái Published (true/false)
            if (searchPublished.HasValue)
            {
                tinTucs = tinTucs.Where(t => t.Published == searchPublished);
            }

            // Lọc theo tên tác giả
            if (!string.IsNullOrEmpty(searchAuthor))
            {
                tinTucs = tinTucs.Where(t => t.Author.Contains(searchAuthor));
            }

            return View(await tinTucs.ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Admin/AdminTintucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs
                .Include(t => t.Account)
                .Include(t => t.Cat)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (tintuc == null)
            {
                return NotFound();
            }

            return View(tintuc);
        }

        [HttpPost]
        public JsonResult UploadImage(IFormFile upload)
        {
            return Helper.UploadImage.Upload(upload); // Gọi đúng phương thức Upload
        }

        // GET: Admin/AdminTintucs/Create
        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "FullName");
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tintuc tintuc, IFormFile? ThumbFile)
        {
            ViewData["SecondPage"] = "Danh sách tin tức";
            ViewData["SecondController"] = "AdminTintucs";
            ViewData["SecondAction"] = "Index";
            ViewData["CurrentPage"] = "Tạo tin tức";

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = new SelectList(_context.Categories, "CatId", "CatName", tintuc.CatId);
                    ViewBag.Accounts = new SelectList(_context.Accounts, "AccountId", "FullName", tintuc.AccountId);
                    return View(tintuc);
                }

                // Xử lý ảnh đại diện
                tintuc.Thum = ThumbFile != null ? await FileHelper.SaveFileAsync(ThumbFile) : "default-thumbnail.jpg";

                // Tạo Alias tự động
                tintuc.Alias ??= AliasHelper.GenerateAlias(tintuc.Title);
                tintuc.MetaDesc ??= SeoHelper.GenerateMetaDescription(tintuc.Title, tintuc.Scontents ?? "", tintuc.Contents ?? "");
                tintuc.MetaKey ??= SeoHelper.GenerateMetaKeywords(tintuc.Title, tintuc.Scontents ?? "");
                // Ngày tạo mặc định là hiện tại
                tintuc.CreatedDate = DateTime.UtcNow;
                tintuc.Views = 0;

                // Lưu vào database
                _context.Add(tintuc);
                await _context.SaveChangesAsync();

                _notyf.Success("🎉 Thêm tin tức thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"❌ Lỗi: {ex.Message}");
                _notyf.Warning($"tác giả: {tintuc.Author}");
                ViewBag.Categories = new SelectList(_context.Categories, "CatId", "CatName", tintuc.CatId);
                ViewBag.Accounts = new SelectList(_context.Accounts, "AccountId", "FullName", tintuc.AccountId);
                return View(tintuc);
            }
        }

        // GET: Admin/AdminTintucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs.FindAsync(id);
            if (tintuc == null)
            {
                return NotFound();
            }

            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "FullName", tintuc.AccountId);
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", tintuc.CatId);
            return View(tintuc);
        }

        // POST: Admin/AdminTintucs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tintuc tintuc, IFormFile? ThumbFile)
        {
            if (id != tintuc.PostId)
            {
                _notyf.Error("Tin tức không tồn tại!");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "FullName", tintuc.AccountId);
                ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", tintuc.CatId);
                _notyf.Warning("Vui lòng kiểm tra lại thông tin tin tức.");
                return View(tintuc);
            }

            try
            {
                var existingTintuc = await _context.Tintucs.FindAsync(id);
                if (existingTintuc == null)
                {
                    _notyf.Error("Tin tức không tồn tại!");
                    return NotFound();
                }

                // Cập nhật tất cả thông tin
                existingTintuc.Title = tintuc.Title;
                existingTintuc.Scontents = tintuc.Scontents;
                existingTintuc.Contents = tintuc.Contents;
                existingTintuc.Alias = tintuc.Alias;
                existingTintuc.CreatedDate = tintuc.CreatedDate;
                existingTintuc.Author = tintuc.Author;
                existingTintuc.AccountId = tintuc.AccountId;
                existingTintuc.Tags = tintuc.Tags;
                existingTintuc.CatId = tintuc.CatId;
                existingTintuc.IsHot = tintuc.IsHot;
                existingTintuc.IsNewfeed = tintuc.IsNewfeed;
                existingTintuc.MetaKey = tintuc.MetaKey;
                existingTintuc.MetaDesc = tintuc.MetaDesc;
                existingTintuc.Views = tintuc.Views;
                existingTintuc.Published = tintuc.Published;

                // Xử lý upload ảnh mới
                if (ThumbFile != null)
                {
                    Console.WriteLine($"Old Thumb: {existingTintuc.Thum}");
                    FileHelper.DeleteFile(existingTintuc.Thum); // Xóa ảnh cũ
                    existingTintuc.Thum = await FileHelper.SaveFileAsync(ThumbFile); // Lưu ảnh mới
                    Console.WriteLine($"New Thumb: {existingTintuc.Thum}");
                }

                // Cập nhật database
                _context.Update(existingTintuc);
                await _context.SaveChangesAsync();
                _notyf.Success("🎉 Cập nhật tin tức thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"❌ Lỗi: {ex.Message}");
                ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "FullName", tintuc.AccountId);
                ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", tintuc.CatId);
                return View(tintuc);
            }
        }

        // GET: Admin/AdminTintucs/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.Tintucs.FindAsync(id);
            if (news == null)
            {
                return Json(new { success = false, message = "Tin tức không tồn tại!" });
            }

            _context.Tintucs.Remove(news);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Tin tức đã bị xóa!" });
        }
        private bool TintucExists(int id)
        {
            return _context.Tintucs.Any(e => e.PostId == id);
        }
    }
}
