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
            string fileName = DateTime.Now.Ticks.ToString() + "_profile_" + imageFile.FileName;
            await UploadImageAsync(directoryPath, imageFile, fileName);
            return fileName;
        }
        public async Task<string> UploadBlogImageAsync(IFormFile imageFile)
        {
            string directoryPath = Path.Combine(_webHostEnv.WebRootPath, "images", "blogs");
            string fileName = DateTime.Now.Ticks.ToString() + "_blog_" + imageFile.FileName;
            await UploadImageAsync(directoryPath, imageFile, fileName);
            return fileName;
        }
        private async Task UploadImageAsync(
            string directoryPath, 
            IFormFile imageFile,
            string formattedFileName)
        {
            //TODO: place images and profiles to config
            string filePath = Path.Combine(directoryPath, formattedFileName);
            _logger.LogInformation($"File path of uploaded image is {filePath}");
            using (var stream = File.Create(filePath))
            {
                await imageFile.CopyToAsync(stream);
            }
        }
        public void DeleteImage(string fileName)
        {
            //TODO: move default iamge name to config
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
    }
}
