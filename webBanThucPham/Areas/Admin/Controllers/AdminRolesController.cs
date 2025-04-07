using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using webBanThucPham.Models;

namespace webBanThucPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminRolesController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public AdminRolesController(INotyfService notyf, DbBanThucPhamContext context)
        {
            _notyf = notyf;
            _context = context;
        }


        // GET: Admin/AdminRoles
        public async Task<IActionResult> Index()
        {

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Quản lí quyền truy cập";
            return View(await _context.Roles.ToListAsync());
        }

        // GET: Admin/AdminRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí quyền truy cập";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Thông tin chi tiết";
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleId == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: Admin/AdminRoles/Create
        public IActionResult Create()
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí quyền truy cập";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Thêm quyền truy cập";
            return View();
        }

        // POST: Admin/AdminRoles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleId,RoleName,Description")] Role role)
        {
            if (ModelState.IsValid)
            {
                _context.Add(role);
                await _context.SaveChangesAsync();
                // Them thong bao thanh cong
                _notyf.Success("Tạo vai trò mới thành công! 🎉");
                return RedirectToAction(nameof(Index));
            }
            // Hien thi thong bao loi
            _notyf.Error("Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại!");
            return View(role);
        }

        // GET: Admin/AdminRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí quyền truy cập";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Chỉnh sửa quyền truy cập";
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Admin/AdminRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleId,RoleName,Description")] Role role)
        {
            if (id != role.RoleId)
            {
                _notyf.Error("Không tìm thấy vai trò cần chỉnh sửa!");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();

                    // Thông báo thành công
                    _notyf.Success("Cập nhật vai trò thành công! 🎉");

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.RoleId))
                    {
                        _notyf.Error("Vai trò không tồn tại hoặc đã bị xóa!");
                        return NotFound();
                    }
                    else
                    {
                        _notyf.Error("Đã xảy ra lỗi khi cập nhật vai trò. Vui lòng thử lại!");
                        throw;
                    }
                }
            }

            // Thông báo lỗi nếu ModelState không hợp lệ
            _notyf.Error("Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại!");

            return View(role);
        }


        // GET: Admin/AdminRoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí quyền truy cập";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Xóa quyền truy cập";
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleId == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Admin/AdminRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                // Thông báo khi xóa thành công
                _notyf.Success("Xóa vai trò thành công! 🗑️");
            }
            else
            {
                // Thông báo khi không tìm thấy vai trò
                _notyf.Error("Không tìm thấy vai trò cần xóa!");
            }

            return RedirectToAction(nameof(Index));
        }


        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.RoleId == id);
        }
    }
}
