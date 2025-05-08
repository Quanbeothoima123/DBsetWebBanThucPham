using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;
using webBanThucPham.Helper;
using Microsoft.AspNetCore.Hosting;
namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public AdminProductsController(INotyfService notyf, DbBanThucPhamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _notyf = notyf;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/AdminProducts
        public async Task<IActionResult> Index(string searchName, string searchCategory, bool? searchStatus, int? page)
        {
            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Danh sách sản phẩm"; 
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = page ?? 1;

            var products = _context.Products
                                  .Include(p => p.Cat)
                                  .OrderByDescending(p => p.DateCreated)
                                  .AsQueryable();

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.ProductName.Contains(searchName));
            }

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(searchCategory))
            {
                products = products.Where(p => p.Cat != null && p.Cat.CatName == searchCategory);
            }

            // Lọc theo trạng thái (Active)
            if (searchStatus.HasValue)
            {
                products = products.Where(p => p.Active == searchStatus);
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await products.ToPagedListAsync(pageNumber, pageSize));
        }



        // GET: Admin/AdminProducts/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Danh sách sản phẩm";
            ViewData["SecondController"] = "AdminProducts";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Thông tin chi tiết";
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cat) // Load danh mục
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost]
        public JsonResult UploadImage(IFormFile upload)
        {
            return Helper.UploadImage.Upload(upload); // Gọi đúng phương thức Upload
        }

        // GET: Admin/AdminProducts/Create
        public IActionResult Create()
        {
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ThumbFile, List<IFormFile>? DetailImages, IFormFile? VideoFile)
        {
            // Cấp 2: Tên trang và thông tin liên kết
            ViewData["SecondPage"] = "Danh sách sản phẩm";
            ViewData["SecondController"] = "AdminProducts";
            ViewData["SecondAction"] = "Index";

            // Cấp 3: Trang hiện tại
            ViewData["CurrentPage"] = "Tạo sản phẩm";

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
                    return View(product);
                }

                // **Xử lý ảnh đại diện (Thumbnail)**
                product.Thumb = ThumbFile != null ? await FileHelper.SaveFileAsync(ThumbFile) : "default-thumbnail.jpg";

                // **Xử lý Video**
                product.Video = VideoFile != null ? await FileHelper.SaveFileAsync(VideoFile) : null;

                // **Xử lý ảnh chi tiết (Tối đa 5 ảnh)**
                if (DetailImages != null && DetailImages.Count > 0)
                {
                    List<string> imagePaths = new List<string>();
                    foreach (var file in DetailImages)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string savedPath = await FileHelper.SaveFileAsync(file);
                            imagePaths.Add(savedPath);
                        }
                    }
                    product.Thumbnail = string.Join(" ", imagePaths); // Lưu đường dẫn ảnh cách nhau bởi dấu cách
                }

                // **Không cần xử lý ảnh trong Description**, vì CKEditor đã upload sẵn vào `wwwroot/uploads`

                // **Tạo Meta tự động**
                product.Alias = AliasHelper.GenerateAlias(product.ProductName);
                product.MetaDesc = SeoHelper.GenerateMetaDescription(product.ProductName, product.ShortDesc ?? "", product.Description ?? "");
                product.MetaKey = SeoHelper.GenerateMetaKeywords(product.ProductName, product.ShortDesc ?? "");

                // **Lưu sản phẩm vào database**
                _context.Add(product);
                await _context.SaveChangesAsync();

                _notyf.Success("🎉 Thêm sản phẩm thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"❌ Lỗi: {ex.Message}");
                ViewBag.Categories = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
                return View(product);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Danh sách sản phẩm";
            ViewData["SecondController"] = "AdminProducts";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Chỉnh sửa sản phẩm";
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile ThumbFile,
     List<IFormFile> NewThumbFiles, string DeleteIndexesRaw = "")
        {
            if (id != product.ProductId)
            {
                _notyf.Error("Sản phẩm không tồn tại!");
                return NotFound();
            }

            ModelState.Remove("ThumbFile");

            if (!ModelState.IsValid)
            {
                _notyf.Warning("Vui lòng kiểm tra lại thông tin sản phẩm.");
                ViewBag.Categories = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
                return View(product);
            }

            try
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    _notyf.Error("Sản phẩm không tồn tại!");
                    return NotFound();
                }

                // **Cập nhật thông tin sản phẩm**
                existingProduct.ProductName = product.ProductName;
                existingProduct.CatId = product.CatId;
                existingProduct.ShortDesc = product.ShortDesc;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Discount = product.Discount;
                existingProduct.UnitsInStock = product.UnitsInStock;
                existingProduct.Active = product.Active;

                // **Cập nhật ảnh chính (Thumb)**
                if (ThumbFile != null)
                {
                    if (!string.IsNullOrEmpty(existingProduct.Thumb))
                    {
                        FileHelper.DeleteFile(existingProduct.Thumb);
                    }
                    existingProduct.Thumb = await FileHelper.SaveFileAsync(ThumbFile);
                }

                // **Xử lý ảnh phụ (Thumbnail)**
                List<string> existingThumbnails = existingProduct.Thumbnail?.Split(" ").ToList() ?? new List<string>();

                // 🔴 **Ghi nhớ vị trí cần xóa**
                if (!string.IsNullOrWhiteSpace(DeleteIndexesRaw))
                {
                    List<int> deleteIndexes = DeleteIndexesRaw.Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int x) ? x : -1)
                        .Where(x => x >= 0 && x < existingThumbnails.Count)
                        .OrderByDescending(x => x) // Xóa từ cuối về đầu để tránh thay đổi index
                        .ToList();

                    foreach (int index in deleteIndexes)
                    {
                        FileHelper.DeleteFile(existingThumbnails[index]); // Xóa file vật lý
                        existingThumbnails.RemoveAt(index); // Xóa khỏi danh sách
                    }
                }

                // 🆕 **Thêm ảnh mới vào cuối danh sách**
                foreach (var file in NewThumbFiles)
                {
                    existingThumbnails.Add(await FileHelper.SaveFileAsync(file));
                }

                // 🗑 **Nếu không còn ảnh nào thì đặt Thumbnail rỗng**
                existingProduct.Thumbnail = existingThumbnails.Count > 0 ? string.Join(" ", existingThumbnails) : "";

                // **Lưu vào database**
                _context.Update(existingProduct);
                await _context.SaveChangesAsync();

                _notyf.Success("🎉 Cập nhật sản phẩm thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"❌ Lỗi: {ex.Message}");
                ViewBag.Categories = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
                return View(product);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại." });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sản phẩm đã được xóa thành công." });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
