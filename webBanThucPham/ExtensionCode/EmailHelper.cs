using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace webBanThucPham.ExtensionCode
{
    public static class EmailHelper
    {
        private static string SendGridApiKey = "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"; // Thay bằng API Key của bạn

        // 📌 HÀM CHUNG DÙNG ĐỂ GỬI EMAIL VỚI NỘI DUNG TÙY CHỈNH
        public static async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var client = new SendGridClient(SendGridApiKey);
                var from = new EmailAddress("phimanhnamquan@gmail.com", "Web Bán Thực Phẩm");
                var to = new EmailAddress(toEmail);
                var plainTextContent = body;
                var htmlContent = $"<strong>{body}</strong>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return response.StatusCode == System.Net.HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                return false;
            }
        }

        // 📌 HÀM GỬI MÃ XÁC NHẬN -> GỌI LẠI SendEmailAsync
        public static async Task<bool> SendVerificationEmail(string toEmail, string verificationCode)
        {
            string subject = "Xác nhận tài khoản";
            string body = $"Mã xác thực của bạn là: <h2>{verificationCode}</h2>";

            // 🎯 GỌI LẠI SendEmailAsync ĐỂ GỬI EMAIL
            return await SendEmailAsync(toEmail, subject, body);
        }

        // 📌 HÀM GỬI MÃ OTP ĐĂNG NHẬP -> GỌI LẠI SendEmailAsync
        public static async Task<bool> SendOTPEmail(string toEmail, string otpCode)
        {
            string subject = "Mã OTP đăng nhập";
            string body = $"Mã OTP của bạn là: <h2>{otpCode}</h2>";

            // 🎯 GỌI LẠI SendEmailAsync ĐỂ GỬI EMAIL
            return await SendEmailAsync(toEmail, subject, body);
        }
    }

}
