using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using BlogApp.Interfaces;

namespace BlogApp.Services
{
    [Obsolete]
    public class LegacyImageStorage : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly ILogger<LegacyImageStorage> _logger;

        public LegacyImageStorage(
            IWebHostEnvironment webHostEnv,
            ILogger<LegacyImageStorage> logger)
        {
            _webHostEnv = webHostEnv;
            _logger = logger;
        }

        public async Task UploadProfileImageAsync(IFormFile imageFile, string fileName)
        {
            string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "profiles");
            await UploadImageAsync(directoryPath, imageFile, fileName);
        }

        public async Task UploadBlogImageAsync(IFormFile imageFile, string fileName)
        {
            string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "blogs");
            await UploadImageAsync(directoryPath, imageFile, fileName);
        }

        public void DeleteImage(string fileName)
        {
            if (fileName != "default.jpg" && fileName != string.Empty)
            {
                string directoryPath = Path
                    .Combine(_webHostEnv.WebRootPath, "images", "profiles");
                string filePath = Path
                    .Combine(directoryPath, fileName);
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"File path of deleted image is {filePath}");
                }
                catch
                {
                    _logger.LogError($"Failed to remove profile picture with file path: ${filePath}");
                }
            }
        }

        public string BuildFileName(string originalName)
        {
            return string.Join
            (
                "_",
                new string[]
                {
                    DateTime.Now.Ticks.ToString(),
                    originalName
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
            using var stream = File.Create(filePath);
            await imageFile.CopyToAsync(stream);
        }
    }
}