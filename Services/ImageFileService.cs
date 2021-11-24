using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace BlogApp.Services
{
    public class ImageFileService
    {
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly ILogger<ImageFileService> _logger; 
        public ImageFileService(IWebHostEnvironment webHostEnv, ILogger<ImageFileService> logger)
        {
            _webHostEnv = webHostEnv;
            _logger = logger;
        }
        public async Task<string> UploadProfileImageAsync(IFormFile imageFile)
        {
            string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "profiles");
            string fileName = BuildFileName("profile", imageFile.FileName);
            await UploadImageAsync(directoryPath, imageFile, fileName);
            return fileName;
        }
        public async Task<string> UploadBlogImageAsync(IFormFile imageFile)
        {
            string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "blogs");
            string fileName = BuildFileName("blog", imageFile.FileName);
            await UploadImageAsync(directoryPath, imageFile, fileName);
            return fileName;
        }
        public void DeleteImage(string fileName)
        {
            if (fileName != "default" && fileName != string.Empty)
            {
                string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "profiles");
                string filePath = Path.Combine(directoryPath, fileName);
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    _logger.LogError($"Failed to remove profile picture with file path: ${filePath}");
                }
                _logger.LogInformation($"File path of deleted image is {filePath}");
            }
        }
        private string BuildFileName(string type, string fileName)
        {
            return  string.Concat
            (
                "_", 
                new string[] {
                    DateTime.Now.Ticks.ToString(),
                    type,
                    fileName ?? "image"
                }
            );
        }
        private async Task UploadImageAsync(
            string directoryPath, 
            IFormFile imageFile,
            string formattedFileName)
        {
            string filePath = Path.Combine(directoryPath, formattedFileName);
            _logger.LogInformation($"File path of uploaded image is {filePath}");
            using (var stream = File.Create(filePath))
            {
                await imageFile.CopyToAsync(stream);
            }
        }
    }
}
