
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace webBanThucPham.Helper
{
    public static class FileHelper
    {
        private static string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

        public static async Task<string> SaveFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "/uploads/default-thumbnail.jpg";

            EnsureUploadFolderExists();

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Console.WriteLine($"File uploaded: {filePath}");
            return "/uploads/" + fileName;
        }

        public static async Task<List<string>> SaveMultipleFilesAsync(List<IFormFile> files)
        {
            List<string> fileUrls = new();
            if (files == null || files.Count == 0)
                return fileUrls;

            EnsureUploadFolderExists();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileUrls.Add("/uploads/" + fileName);
                }
            }

            return fileUrls;
        }

        public static void DeleteFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || filePath == "/uploads/default-thumbnail.jpg")
                return;

            EnsureUploadFolderExists();

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Console.WriteLine($"File deleted: {fullPath}");
            }
            else
            {
                Console.WriteLine($"File not found: {fullPath}");
            }
        }

        public static void DeleteMultipleFiles(List<string>? filePaths)
        {
            if (filePaths == null || filePaths.Count == 0)
                return;

            foreach (var filePath in filePaths)
            {
                DeleteFile(filePath);
            }
        }

        private static void EnsureUploadFolderExists()
        {
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
        }
    }
}
