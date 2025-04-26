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
    public class AdminOrdersController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public AdminOrdersController(INotyfService notyf, DbBanThucPhamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _notyf = notyf;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // Controller Action
        public async Task<IActionResult> Index(int? orderId, string? customerEmail, int? statusId, DateTime? fromDate, DateTime? toDate, int? minTotal, int? maxTotal, bool? deletedOnly, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TransactStatus)
                .Include(o => o.Orderdetails)
                .AsQueryable();

            // Nếu chọn checkbox "Hủy bởi khách hàng" => chỉ lấy đơn bị hủy
            if (deletedOnly.HasValue && deletedOnly.Value)
            {
                orders = orders.Where(o => o.Deleted);
            }
            else
            {
                // Ngược lại: chỉ lấy đơn chưa bị hủy
                orders = orders.Where(o => !o.Deleted);
            }

            if (orderId.HasValue)
            {
                orders = orders.Where(o => o.OrderId == orderId.Value);
            }

            if (!string.IsNullOrEmpty(customerEmail))
            {
                orders = orders.Where(o => o.Customer.Email.Contains(customerEmail));
            }

            if (statusId.HasValue)
            {
                orders = orders.Where(o => o.TransactStatusId == statusId);
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate);
            }

            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= toDate);
            }

            if (minTotal.HasValue)
            {
                orders = orders.Where(o => o.Orderdetails.Sum(od => od.Total) >= minTotal);
            }

            if (maxTotal.HasValue)
            {
                orders = orders.Where(o => o.Orderdetails.Sum(od => od.Total) <= maxTotal);
            }

            ViewBag.StatusList = new SelectList(await _context.Transactstatusses.ToListAsync(), "TracsactStatusId", "Status");

            return View(await orders.OrderByDescending(o => o.OrderDate).ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: AdminOrders/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.TransactStatus)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            ViewBag.StatusList = new SelectList(_context.Transactstatusses, "TracsactStatusId", "Status");
            return View(order);
        }

        // POST: AdminOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,TransactStatusId,ShipDate,PaymentDate")] Order updatedOrder)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            order.TransactStatusId = updatedOrder.TransactStatusId;
            order.ShipDate = updatedOrder.ShipDate;
            order.PaymentDate = updatedOrder.PaymentDate;

            await _context.SaveChangesAsync();

            _notyf.Success("✅ Cập nhật đơn hàng thành công!");
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.Customer)
                .Include(o => o.PaymentMethod)
                .Include(o => o.TransactStatus)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return NotFound();
            }

            return View(order);
        }


    }
}
