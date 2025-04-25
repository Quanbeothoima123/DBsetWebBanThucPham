using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using webBanThucPham.Models;
using webBanThucPham.ExtensionCode;
using AspNetCoreHero.ToastNotification.Abstractions;
using webBanThucPham.Models.ViewModels;
using webBanThucPham.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace webBanThucPham.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DbBanThucPhamContext _context;
        private readonly INotyfService _notyf;
        public OrdersController(INotyfService notyf, DbBanThucPhamContext context)
        {
            _context = context;
            _notyf = notyf;
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.Customer) // 👈 Thêm dòng này
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


    }
}
