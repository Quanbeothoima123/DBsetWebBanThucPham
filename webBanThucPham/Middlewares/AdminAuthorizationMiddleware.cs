using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace webBanThucPham.Middlewares
{
    public class AdminAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();

            // Bỏ qua kiểm tra nếu là trang đăng nhập, gửi OTP, xác thực OTP
            if (path.StartsWith("/admin/adminaccounts/login") ||
                path.StartsWith("/admin/adminaccounts/sendotp") ||
                path.StartsWith("/admin/adminaccounts/verifyotp"))
            {
                await _next(context);
                return;
            }

            // Nếu là các route trong /admin mà chưa đăng nhập, redirect về trang login
            if (path.StartsWith("/admin"))
            {
                var email = context.Session.GetString("AdminEmail");
                var roleId = context.Session.GetInt32("RoleId");

                if (string.IsNullOrEmpty(email) || roleId != 1)
                {
                    context.Response.Redirect("/Admin/AdminAccounts/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
