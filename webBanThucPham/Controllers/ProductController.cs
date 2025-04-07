using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;
using X.PagedList;
namespace webBanThucPham.Controllers
{
    public class ProductController : Controller
    {
        private readonly DbBanThucPhamContext _context;

        // Inject DbContext vào controller
        public ProductController(DbBanThucPhamContext context)
        {
            _context = context;
        }
        public IActionResult Index(string search, bool? discounted, bool? newProducts, bool? inStock, int? minPrice, int? maxPrice, int sortOrder = 1, int page = 1)
        {
            int pageSize = 10; // Số sản phẩm mỗi trang

            var products = _context.Products.AsQueryable();

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            // Lọc sản phẩm giảm giá
            if (discounted == true)
            {
                products = products.Where(p => p.Discount > 0);
            }

            // Lọc sản phẩm mới (giả sử trong 30 ngày gần nhất)
            if (newProducts == true)
            {
                var recentDate = DateTime.Now.AddDays(-30);
                products = products.Where(p => p.DateCreated >= recentDate);
            }

            // Lọc sản phẩm còn hàng
            if (inStock == true)
            {
                products = products.Where(p => p.UnitsInStock > 0);
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
                _ => products.OrderByDescending(p => p.DateCreated), // Mặc định: ngày tạo mới nhất
            };

            // Phân trang với X.PagedList
            var pagedProducts = products.ToPagedList(page, pageSize);

            ViewBag.Search = search;
            ViewBag.Discounted = discounted;
            ViewBag.NewProducts = newProducts;
            ViewBag.InStock = inStock;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalCount = products.Count();
            // Lấy danh sách tags từ sản phẩm
            var tags = _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Tags)) // Chỉ lấy sản phẩm có tags
                .Select(p => p.Tags) // Chỉ lấy cột Tags
                .ToList() // Chuyển dữ liệu về RAM để xử lý tiếp
                .SelectMany(tags => tags.Split(',')) // Tách chuỗi Tags thành danh sách
                .Distinct() // Lọc trùng
                .ToList(); // Chuyển thành danh sách hoàn chỉnh

            ViewBag.Tags = tags;


            return View(pagedProducts);
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.Cat) // Nạp thêm thông tin danh mục
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy 5 sản phẩm cùng danh mục
            var relatedProducts = _context.Products
                .Where(p => p.CatId == product.CatId && p.ProductId != id)
                .OrderByDescending(p => p.DateCreated)
                .Take(5)
                .ToList();

            // Lấy 5 sản phẩm bán chạy
            var bestSellers = _context.Products
                .Where(p => p.BestSellers == true)
                .OrderByDescending(p => p.DateCreated)
                .Take(5)
                .ToList();

            // Gán vào ViewBag
            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.BestSellers = bestSellers;

            return View(product);
        }
        public IActionResult RelatedProducts(string categoryName, string search, bool? discounted, bool? newProducts, bool? inStock, int? minPrice, int? maxPrice, int sortOrder = 1, int page = 1)
        {
            int pageSize = 10; // Số sản phẩm mỗi trang

            var products = _context.Products.AsQueryable();

            // 🔹 Lọc theo danh mục (nếu có categoryName)
            if (!string.IsNullOrEmpty(categoryName))
            {
                var category = _context.Categories.FirstOrDefault(c => c.CatName == categoryName);
                if (category != null)
                {
                    products = products.Where(p => p.CatId == category.CatId);
                }
            }

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            // Lọc sản phẩm giảm giá
            if (discounted == true)
            {
                products = products.Where(p => p.Discount > 0);
            }

            // Lọc sản phẩm mới (giả sử trong 30 ngày gần nhất)
            if (newProducts == true)
            {
                var recentDate = DateTime.Now.AddDays(-30);
                products = products.Where(p => p.DateCreated >= recentDate);
            }

            // Lọc sản phẩm còn hàng
            if (inStock == true)
            {
                products = products.Where(p => p.UnitsInStock > 0);
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
                _ => products.OrderByDescending(p => p.DateCreated), // Mặc định: ngày tạo mới nhất
            };

            // Phân trang với X.PagedList
            var pagedProducts = products.ToPagedList(page, pageSize);

            ViewBag.Search = search;
            ViewBag.Discounted = discounted;
            ViewBag.NewProducts = newProducts;
            ViewBag.InStock = inStock;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalCount = products.Count();
            // Lấy danh sách tags từ sản phẩm
            var tags = _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Tags)) // Chỉ lấy sản phẩm có tags
                .Select(p => p.Tags) // Chỉ lấy cột Tags
                .ToList() // Chuyển dữ liệu về RAM để xử lý tiếp
                .SelectMany(tags => tags.Split(',')) // Tách chuỗi Tags thành danh sách
                .Distinct() // Lọc trùng
                .ToList(); // Chuyển thành danh sách hoàn chỉnh

            ViewBag.Tags = tags;


            return View(pagedProducts);
        }
    }
 }

