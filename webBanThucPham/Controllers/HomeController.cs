using System.Diagnostics;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
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
