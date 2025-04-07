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
    public class AdminAccountsController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly DbBanThucPhamContext _context;

        public AdminAccountsController(INotyfService notyf, DbBanThucPhamContext context)
        {
            _notyf = notyf;
            _context = context;
        }

        // GET: Admin/AdminAccounts
        public async Task<IActionResult> Index()
        {
            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Quản lí tài khoản";
            // Lấy danh sách quyền từ database
            ViewBag.Roles = _context.Roles
                                    .Select(r => new SelectListItem
                                    {
                                        Value = r.RoleId.ToString(),
                                        Text = r.RoleName
                                    })
                                    .ToList();

            // Danh sách trạng thái
            ViewBag.Statuses = new List<SelectListItem>
    {
        new SelectListItem { Value = "true", Text = "Hoạt động" },
        new SelectListItem { Value = "false", Text = "Bị khóa" }
    };
            var dbBanThucPhamContext = _context.Accounts.Include(a => a.Role);
            return View(await dbBanThucPhamContext.ToListAsync());
        }

        // GET: Admin/AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí tài khoản";
            ViewData["SecondController"] = "AdminAccounts";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Thông tin chi tiết";
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí tài khoản";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Tạo tài khoản";
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,Phone,Email,Password,Salt,Active,FullName,RoleId,CreateDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                // Gửi thông báo thành công
                _notyf.Success("Tài khoản đã được tạo thành công!");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Đã có lỗi xảy ra!");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId", account.RoleId);
            return View(account);
        }

        // GET: Admin/AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí tài khoản";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Chỉnh sửa tài khoản";
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,Phone,Email,Password,Salt,Active,FullName,RoleId,CreateDate")] Account account)
        {
            if (id != account.AccountId)
            {
                _notyf.Error("Không tìm thấy tài khoản cần chỉnh sửa!");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();

                    _notyf.Success("Cập nhật tài khoản thành công!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        _notyf.Error("Tài khoản không tồn tại!");
                        return NotFound();
                    }
                    else
                    {
                        _notyf.Error("Lỗi xung đột dữ liệu, vui lòng thử lại!");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _notyf.Warning("Có lỗi xảy ra, vui lòng kiểm tra lại thông tin!");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", account.RoleId);
            return View(account);
        }


        // GET: Admin/AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Cap 2: Ten trang va thong tin lien ket
            ViewData["SecondPage"] = "Quản lí tài khoản";
            ViewData["SecondController"] = "AdminRoles";
            ViewData["SecondAction"] = "Index";

            // Cap 3:trang hien tai
            ViewData["CurrentPage"] = "Xóa tài khoản";
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa tài khoản thành công!");
            }
            else
            {
                _notyf.Error("Không tìm thấy tài khoản để xóa!");
            }

            return RedirectToAction(nameof(Index));
        }


        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
