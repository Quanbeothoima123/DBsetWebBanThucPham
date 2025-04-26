using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using webBanThucPham.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using webBanThucPham.Models.ViewModel;
namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public HomeController(INotyfService notyf, DbBanThucPhamContext context)
        {
            _notyf = notyf;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var newOrdersToday = await _context.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == DateTime.Today)
                .CountAsync();

            var totalCustomers = await _context.Customers.CountAsync();

            var deliveredOrdersToday = await _context.Orders
                .Where(o => o.ShipDate.HasValue && o.ShipDate.Value.Date == DateTime.Today && o.TransactStatusId == 5)
                .CountAsync();

            var totalRevenueToday = await _context.Orderdetails
                .Where(od => od.Order.OrderDate.HasValue && od.Order.OrderDate.Value.Date == DateTime.Today)
                .SumAsync(od => (decimal?)od.Total) ?? 0;

            var totalProducts = await _context.Products.CountAsync();

            // 👉 Phần quan trọng: lấy số lượng đơn theo từng TransactStatusId
            var orderStatusData = await _context.Orders
                .Where(o => o.TransactStatusId != null) // chỉ lấy đơn có trạng thái xác định
                .GroupBy(o => o.TransactStatusId.Value) // Group theo TransactStatusId
                .Select(g => new OrderStatusData
                {
                    StatusId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate.HasValue)
                .OrderByDescending(o => o.OrderDate)
                .Take(20)
                .ToListAsync();

            var top5BestSellingProducts = await _context.Orderdetails
                .Include(od => od.Product)
                .ThenInclude(p => p.Cat)
                .GroupBy(od => od.ProductId)
                .OrderByDescending(g => g.Sum(od => od.Quantity))
                .Take(5)
                .Select(g => new TopProduct
                {
                    Product = g.FirstOrDefault().Product,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .ToListAsync();

            var top5LowStockProducts = await _context.Products
                .Include(p => p.Cat)
                .OrderBy(p => p.UnitsInStock)
                .Take(5)
                .ToListAsync();

            var top5DiscountedProducts = await _context.Products
                .Include(p => p.Cat)
                .Where(p => p.Discount > 0)
                .OrderByDescending(p => p.Discount)
                .Take(5)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                NewOrdersToday = newOrdersToday,
                TotalCustomers = totalCustomers,
                DeliveredOrdersToday = deliveredOrdersToday,
                TotalRevenueToday = totalRevenueToday,
                TotalProducts = totalProducts,
                OrderStatusData = orderStatusData,
                RecentOrders = recentOrders,
                Top5BestSellingProducts = top5BestSellingProducts,
                Top5LowStockProducts = top5LowStockProducts,
                Top5DiscountedProducts = top5DiscountedProducts
            };

            return View(viewModel);
        }


    }
}
