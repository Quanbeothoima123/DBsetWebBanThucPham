using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;

namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCustomersController : Controller
    {
        private readonly DbBanThucPhamContext _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment; // <-- Thêm dòng này

        public AdminCustomersController(DbBanThucPhamContext context, INotyfService notyf, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notyf = notyf;
            _webHostEnvironment = webHostEnvironment; // <-- Gán giá trị cho biến này
            //Console.WriteLine("Giá trị của _webHostEnvironment: " + (_webHostEnvironment == null ? "NULL" : "Có dữ liệu"));
        }


        // GET: Admin/AdminCustomers
        public async Task<IActionResult> Index(int? page)
        {
            ViewData["CurrentPage"] = "Quản lý khách hàng"; // Tiêu đề trang

            int pageSize = 5; // Số khách hàng trên mỗi trang
            int pageNumber = page ?? 1; // Nếu không có tham số `page`, mặc định là trang 1

            var customers = await _context.Customers
                                          .Include(c => c.Location)
                                          .OrderByDescending(c => c.CreateDate)
                                          .ToPagedListAsync(pageNumber, pageSize);

            return View(customers);
        }
        // GET: Admin/AdminCustomers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí khách hàng";
            ViewData["SecondController"] = "AdminCustomers";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Thông tin chi tiết";
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Location)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Admin/AdminCustomers/Create
        public IActionResult Create()
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí khách hàng";
            ViewData["SecondController"] = "AdminCustomers";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Tạo thêm khách hàng";
            ViewData["LocationId"] = new SelectList(_context.Locations, "LocationId", "LocationId");
            return View();
        }

        // POST: Admin/AdminCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)] // Giới hạn 50MB
        public async Task<IActionResult> Create([Bind("FullName,Birthday,Address,Email,Phone,Password,LocationId,Active")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customer.CreateDate = DateTime.Now;
                    _context.Add(customer);
                    await _context.SaveChangesAsync();

                    _notyf.Success("Thêm khách hàng thành công!");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lưu khách hàng: {ex.Message}");
                    _notyf.Error("Lỗi xảy ra, vui lòng thử lại!");
                    return View(customer);
                }
            }

            _notyf.Error("Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại!");
            return View(customer);
        }

        // GET: Admin/AdminCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí khách hàng";
            ViewData["SecondController"] = "AdminCustomers";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Chỉnh sửa thông tin";
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            ViewData["LocationId"] = new SelectList(_context.Locations, "LocationId", "LocationId", customer.LocationId);
            return View(customer);
        }

        // POST: Admin/AdminCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Birthday,Avatar,Address,Email,Phone,LocationId,District,Ward,CreateDate,Password,Salt,LastLogin,Active")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                _notyf.Warning("Không tìm thấy khách hàng cần chỉnh sửa!");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Cập nhật thông tin khách hàng thành công!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        _notyf.Error("Khách hàng không tồn tại!");
                        return NotFound();
                    }
                    else
                    {
                        _notyf.Error("Có lỗi xảy ra khi cập nhật, vui lòng thử lại!");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _notyf.Error("Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");

            ViewData["LocationId"] = new SelectList(_context.Locations, "LocationId", "LocationId", customer.LocationId);
            return View(customer);
        }


        // GET: Admin/AdminCustomers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Location)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Admin/AdminCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa khách hàng thành công!");
            }
            else
            {
                _notyf.Error("Không tìm thấy khách hàng để xóa!");
            }

            return RedirectToAction(nameof(Index));
        }


        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
