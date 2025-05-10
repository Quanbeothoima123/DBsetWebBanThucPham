using System.Diagnostics;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;

namespace webBanThucPham.Controllers;

public class HomeController : Controller
{

    private readonly INotyfService _notyf;
    private readonly DbBanThucPhamContext _context;
    private readonly ILogger<HomeController> _logger;
    public HomeController(INotyfService notyf, DbBanThucPhamContext context, ILogger<HomeController> logger)
    {
        _notyf = notyf;
        _context = context;
        _logger = logger;
    }
    public IActionResult Index()
    {
        var userCustomID = HttpContext.Session.GetInt32("CustomerId");

        // Neu nguoi dung da dang nhap , lay thong tin cua gio hang
        if (userCustomID.HasValue)
        {
            var cartItems = _context.Cartitems
                .Where(ci => ci.Cart.CustomerId == userCustomID)
                .Include(ci => ci.Product)  // Bao gom thong tin san pham
                .ToList();

            // Tinh tong gia tri cua gio hang
            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Price);

            // Truyen gio hang vao  ViewBag, bao gom thong tin Thumb tu Product
            ViewBag.CartItems = cartItems.Select(ci => new
            {
                ci.CartItemId,
                ci.ProductId,
                ci.Quantity,
                ci.Price,
                ProductName = ci.Product.ProductName,
                ProductThumb = ci.Product.Thumb,  // Lay Thumb tu Product
                ProductPrice = ci.Product.Price
            }).ToList();

            ViewBag.TotalAmount = totalAmount;
        }
        else
        {
            //Neu chua dang nhap gio hang se trong
            ViewBag.CartItems = new List<object>();  // De tranh loi khi gio hang trong
            ViewBag.TotalAmount = 0;
        }


        var categories = _context.Categories
            .Where(c => c.CatId >= 1 && c.CatId <= 10)
            .ToList();

        ViewBag.Categories = categories;

        var productsByCategory = new Dictionary<int, List<Product>>();
        foreach (var category in categories)
        {
            var products = _context.Products
                .Where(p => p.CatId == category.CatId && p.Active == true)
                .OrderByDescending(p => p.DateCreated)
                .ToList(); //Lay toan bo san pham cua danh muc do

            productsByCategory[category.CatId] = products;
        }

        ViewBag.ProductsByCategory = productsByCategory;

        // Lay danh sach bai viet moi (IsNewfeed = true và Published = true)
        var newfeedPosts = _context.Tintucs
            .Where(t => t.IsNewfeed == true && t.Published == true)
            .OrderByDescending(t => t.CreatedDate)
            .ToList();

        ViewBag.NewfeedPosts = newfeedPosts;
        return View();
    }

    public IActionResult Contact(int? id)
    {
        // Nếu không có id được truyền vào, mặc định hiển thị PageId = 4
        int pageId = id ?? 2;

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
    public IActionResult About(int? id)
    {
        // Nếu không có id được truyền vào, mặc định hiển thị PageId = 4
        int pageId = id ?? 25;

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

    public IActionResult Privacy()
    {
        return View();
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
