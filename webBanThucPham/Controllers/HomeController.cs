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

    public IActionResult Contact()
    {
        return View();
    }
    public IActionResult About()
    {
        return View();
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
