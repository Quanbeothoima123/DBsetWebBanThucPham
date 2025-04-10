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

            string hashedPassword = (password + user.Salt).ToMD5();

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

            // Cập nhật LastLogin
            user.LastLogin = DateTime.Now;
            _context.SaveChanges();

            // Lưu Session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetInt32("CustomerId", user.CustomerId);

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<JsonResult> SendOtp([FromBody] EmailViewModel model)
        {
            var user = _context.Customers.FirstOrDefault(c => c.Email == model.Email);
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString());


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
            var otpTimeStr = HttpContext.Session.GetString("OtpTime");
            if (!DateTime.TryParse(otpTimeStr, out DateTime otpTime) || DateTime.Now > otpTime.AddMinutes(5))
            {
                return Json(new { success = false, message = "Mã OTP đã hết hạn. Vui lòng gửi lại mã mới." });
            }


            if (storedOtp == null || storedEmail == null || storedOtp != model.Otp || storedEmail != model.Email)
            {
                return Json(new { success = false, message = "Mã OTP không chính xác." });
            }

            // Tìm user và cập nhật LastLogin
            var user = _context.Customers.FirstOrDefault(c => c.Email == storedEmail);
            if (user != null)
            {
                user.LastLogin = DateTime.Now;
                _context.Customers.Update(user);
                _context.SaveChanges();
            }

            HttpContext.Session.SetString("UserEmail", storedEmail);
            HttpContext.Session.SetString("UserName", user?.FullName ?? ""); // nếu cần
            HttpContext.Session.SetInt32("CustomerId", user?.CustomerId ?? 0);

            HttpContext.Session.Remove("OtpCode");
            HttpContext.Session.Remove("OtpEmail");

            return Json(new { success = true, message = "Đăng nhập thành công!" });
        }

        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ session
            return RedirectToAction("Login", "CustomAccount");
        }

        [HttpGet]
        public IActionResult EditInfo()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var customer = _context.Customers
                .Include(c => c.Deliveryaddresses)
                .FirstOrDefault(c => c.Email == email);

            if (customer == null) return NotFound();

            // Tách địa chỉ mặc định (từ Customer.Address) thành AddressVM
            var addressParts = (customer.Address ?? "").Split('|');
            var defaultAddress = new AddressVM
            {
                Street = addressParts.ElementAtOrDefault(0),
                Ward = addressParts.ElementAtOrDefault(1) ?? "",
                District = addressParts.ElementAtOrDefault(2) ?? "",
                Province = addressParts.ElementAtOrDefault(3) ?? ""
            };

            // Duyệt qua địa chỉ giao hàng
            var deliveryAddresses = customer.Deliveryaddresses.Select(d =>
            {
                var parts = (d.NameAddress ?? "").Split('|');
                return new DeliveryAddressVM
                {
                    DeliveryAddressID = d.DeliveryAddressId,
                    PhoneNumber = d.PhoneNumber ?? "",
                    Address = new AddressVM
                    {
                        Street = parts.ElementAtOrDefault(0),
                        Ward = parts.ElementAtOrDefault(1) ?? "",
                        District = parts.ElementAtOrDefault(2) ?? "",
                        Province = parts.ElementAtOrDefault(3) ?? ""
                    }
                };
            }).ToList();

            var model = new EditInfoViewModel
            {
                FullName = customer.FullName,
                Birthday = customer.Birthday,
                Avatar = customer.Avatar,
                Email = customer.Email ?? "",
                Phone = customer.Phone ?? "",
                LastLogin = customer.LastLogin,
                DefaultAddress = defaultAddress,
                DeliveryAddresses = deliveryAddresses
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult EditInfo(EditInfoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var customer = _context.Customers
                .Include(c => c.Deliveryaddresses)
                .FirstOrDefault(c => c.Email == model.Email);

            if (customer == null)
                return NotFound();

            customer.FullName = model.FullName;
            customer.Birthday = model.Birthday;
            customer.Avatar = model.Avatar;
            customer.Phone = model.Phone;

            // Gộp DefaultAddress thành chuỗi
            customer.Address = string.Join('|', new[]
            {
        model.DefaultAddress.Street,
        model.DefaultAddress.Ward,
        model.DefaultAddress.District,
        model.DefaultAddress.Province
    });

            // Cập nhật DeliveryAddresses
            foreach (var addrVM in model.DeliveryAddresses)
            {
                var delivery = customer.Deliveryaddresses
                    .FirstOrDefault(x => x.DeliveryAddressId == addrVM.DeliveryAddressID);

                if (delivery != null)
                {
                    delivery.PhoneNumber = addrVM.PhoneNumber;
                    delivery.NameAddress = string.Join('|', new[]
                    {
                addrVM.Address.Street,
                addrVM.Address.Ward,
                addrVM.Address.District,
                addrVM.Address.Province
            });
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Thông tin đã được cập nhật!";
            return RedirectToAction("EditInfo");
        }


    }
}
