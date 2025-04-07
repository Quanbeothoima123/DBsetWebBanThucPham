using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using webBanThucPham.Models;
using webBanThucPham.ExtensionCode;
using AspNetCoreHero.ToastNotification.Abstractions;
using webBanThucPham.Models.ViewModels;
using webBanThucPham.Models.ViewModel;

namespace webBanThucPham.Controllers
{
    public class CustomAccountController : Controller
    {
        private readonly DbBanThucPhamContext _context;
        private readonly INotyfService _notyf;
        public CustomAccountController(INotyfService notyf, DbBanThucPhamContext context)
        {
            _context = context;
            _notyf = notyf;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Vui lòng kiểm tra lại thông tin.");
                return View(model);
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = _context.Customers.FirstOrDefault(c => c.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
                return View(model);
            }

            // Gửi email xác thực TRƯỚC khi thêm tài khoản vào database
            string verificationCode = new Random().Next(100000, 999999).ToString();
            bool emailSent = await EmailHelper.SendVerificationEmail(model.Email, verificationCode);

            if (!emailSent)
            {
                ModelState.AddModelError("Email", "Lỗi khi gửi email xác thực. Vui lòng thử lại.");
                return View(model);
            }

            // Nếu gửi email thành công, tiến hành lưu tài khoản vào CSDL
            string salt = SecurityHelper.GetRandomKey();
            string hashedPassword = (model.Password + salt).ToMD5();

            var customer = new Customer
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Birthday = model.Birthday,
                Password = hashedPassword,
                Salt = salt,
                CreateDate = DateTime.Now,
                Active = false // Chờ xác thực email
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Lưu Verification Code vào Session
            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("PendingEmail", model.Email);

            return RedirectToAction("VerifyEmail");
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyEmail(string code)
        {
            string? storedCode = HttpContext.Session.GetString("VerificationCode");
            string? pendingEmail = HttpContext.Session.GetString("PendingEmail");

            if (storedCode == null || pendingEmail == null || storedCode != code)
            {
                TempData["Error"] = "Mã xác thực không đúng, vui lòng thử lại!";
                return RedirectToAction("VerifyEmail"); // Chuyển hướng lại trang VerifyEmail
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == pendingEmail);
            if (customer == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("VerifyEmail");
            }

            customer.Active = true;
            _context.Customers.Update(customer);
            _context.SaveChanges();

            HttpContext.Session.Remove("VerificationCode");
            HttpContext.Session.Remove("PendingEmail");

            return RedirectToAction("Login", "CustomAccount");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Customers.FirstOrDefault(c => c.Email == email);


            if (user == null)
            {
                TempData["Error"] = "Email hoặc mật khẩu không chính xác!";
                return RedirectToAction("Login");
            }
            // Mã hóa mật khẩu nhập vào với Salt từ user
            string hashedPassword = (password+ user.Salt).ToMD5();

            // So sánh mật khẩu đã mã hóa với database
            if (user.Password != hashedPassword)
            {
                TempData["Error"] = "Email hoặc mật khẩu không chính xác!";
                return RedirectToAction("Login");
            }

            if (!user.Active ?? false)
            {
                TempData["Error"] = "Tài khoản chưa được kích hoạt!";
                return RedirectToAction("Login");
            }

            // Lưu thông tin đăng nhập vào Session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetInt32("CustomerId", user.CustomerId);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<JsonResult> SendOtp([FromBody] EmailViewModel model)
        {
            var user = _context.Customers.FirstOrDefault(c => c.Email == model.Email);

            if (user == null || (user.Active.HasValue && !user.Active.Value))
            {
                // xu li tai khoan khong ton tai hoac chua kich hoat
                return Json(new { success = false, message = "Tài khoản không tồn tại hoặc chưa được kích hoạt." });
            }


            Random rnd = new Random();
            string otpCode = rnd.Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OtpCode", otpCode);
            HttpContext.Session.SetString("OtpEmail", model.Email);

            await EmailHelper.SendVerificationEmail(model.Email, otpCode);

            return Json(new { success = true, message = "Mã OTP đã được gửi qua email." });
        }

        [HttpPost]
        public JsonResult VerifyOtp([FromBody] OtpViewModel model)
        {
            string? storedOtp = HttpContext.Session.GetString("OtpCode");
            string? storedEmail = HttpContext.Session.GetString("OtpEmail");

            if (storedOtp == null || storedEmail == null || storedOtp != model.Otp || storedEmail != model.Email)
            {
                return Json(new { success = false, message = "Mã OTP không chính xác." });
            }

            HttpContext.Session.SetString("UserEmail", storedEmail);
            HttpContext.Session.Remove("OtpCode");
            HttpContext.Session.Remove("OtpEmail");

            return Json(new { success = true, message = "Đăng nhập thành công!" });
        }


    }
}
