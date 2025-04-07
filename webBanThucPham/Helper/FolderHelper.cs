using System;
using System.IO;

namespace webBanThucPham.Helper
{
    public static class FolderHelper
    {
        private static readonly string UploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        /// <summary>
        /// Kiểm tra folder uploads có tồn tại không, nếu chưa thì tạo mới.
        /// </summary>
        public static void EnsureUploadsFolderExists()
        {
            try
            {
                if (!Directory.Exists(UploadsFolderPath))
                {
                    Directory.CreateDirectory(UploadsFolderPath);
                    Console.WriteLine("Folder 'uploads' đã được tạo tại: " + UploadsFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tạo folder uploads: " + ex.Message);
            }
        }
    }
}
