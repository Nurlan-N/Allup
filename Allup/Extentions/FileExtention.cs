using Allup.Models;
using Microsoft.AspNetCore.Hosting;

namespace Allup.Extentions
{
    public static class FileExtention
    {
        public static bool CheckFileContentType(this IFormFile file, string contentType)
        {
            return file.ContentType == contentType;
        }
        public static bool CheckFileLength(this IFormFile file, short lenth)
        {
            return (file.Length / 1024) <= lenth;
        }
        public async static Task<string> CreateFileAsync(this IFormFile file,IWebHostEnvironment _webHostEnvironment, params string[] folders)
        {
            string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid().ToString()}-{file.FileName}";

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath);

            foreach (string folder in folders)
            {
                filePath = Path.Combine(filePath, folder);
            }

            filePath = Path.Combine(filePath,fileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}
