using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;
using X.PagedList;

namespace webBanThucPham.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DbBanThucPhamContext _context;

        public CategoryController(DbBanThucPhamContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? catId, string search, bool? discounted, bool? hotProducts, bool? topSold, bool? newProducts, int? minPrice, int? maxPrice, int sortOrder = 1, int page = 1)
        {
            int pageSize = 10; // Số sản phẩm mỗi trang

            // Lấy danh sách danh mục
            var categories = _context.Categories
                .Where(c => c.Published == true)
                .OrderBy(c => c.CatId)
                .ToList();
            ViewBag.Categories = categories;

            // Truy vấn sản phẩm
            var products = _context.Products
                .Where(p => p.Active == true)
                .Include(p => p.Cat)
                .AsQueryable();

            // Lọc theo danh mục
            if (catId.HasValue && catId != 0)
            {
                products = products.Where(p => p.CatId == catId);
            }

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search) || p.Description.Contains(search));
            }

            // Lọc sản phẩm giảm giá
            if (discounted == true)
            {
                products = products.Where(p => p.Discount > 0);
            }

            // Lọc sản phẩm hot (giả sử có trường isHot trong Products)
            if (hotProducts == true)
            {
                products = products.Where(p => p.BestSellers == true); // Cần thêm trường isHot nếu chưa có
            }

            // Lọc sản phẩm bán chạy (dựa trên Orderdetails)
            if (topSold == true)
            {
                var topSoldProductIds = _context.Orderdetails
                    .GroupBy(od => od.ProductId)
                    .OrderByDescending(g => g.Sum(od => od.Quantity))
                    .Select(g => g.Key)
                    .Take(50); // Lấy top 50 sản phẩm bán chạy
                products = products.Where(p => topSoldProductIds.Contains(p.ProductId));
            }

            // Lọc sản phẩm mới (trong 30 ngày gần nhất)
            if (newProducts == true)
            {
                var recentDate = DateTime.Now.AddDays(-30);
                products = products.Where(p => p.DateCreated >= recentDate);
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Sắp xếp sản phẩm
            products = sortOrder switch
            {
                2 => products.OrderByDescending(p => p.Price), // Giá cao -> thấp
                3 => products.OrderBy(p => p.Price), // Giá thấp -> cao
                _ => products.OrderByDescending(p => p.DateCreated), // Mặc định: mới nhất
            };

            // Phân trang
            var pagedProducts = products.ToPagedList(page, pageSize);

            // Truyền dữ liệu vào ViewBag
            ViewBag.SelectedCatId = catId;
            ViewBag.Search = search;
            ViewBag.Discounted = discounted;
            ViewBag.HotProducts = hotProducts;
            ViewBag.TopSold = topSold;
            ViewBag.NewProducts = newProducts;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalCount = products.Count();

            return View(pagedProducts);
        }
    }
}