using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace webBanThucPham.Helper  // Thay YourNamespace bằng namespace thực tế của bạn
{
    public class UploadImage
    {
        public static JsonResult Upload(IFormFile upload)
        {
            try
            {
                if (upload != null && upload.Length > 0)
                {
                    // Tạo tên file duy nhất
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    // Lưu file vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        upload.CopyTo(stream);
                    }

                    // Trả về URL ảnh đã upload
                    var url = $"/uploads/{fileName}";
                    return new JsonResult(new { uploaded = 1, fileName = fileName, url = url });
                }
            }
            catch
            {
                return new JsonResult(new { uploaded = 0, error = new { message = "Lỗi khi tải ảnh lên." } });
            }

            return new JsonResult(new { uploaded = 0, error = new { message = "Không có ảnh nào được chọn." } });
        }
    }
}
