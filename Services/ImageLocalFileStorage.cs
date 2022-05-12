using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlogApp.Services
{
    //todo: move to enum
    public enum ImageType
    {
        BlogCover,
        ProfilePicture
    }

    public class ImageLocalFileStorage : IImageStorage
    {
        private readonly string imageDirectoryName = "images";
        private readonly string defaultProfilePictureName = "default.jpg";
        private readonly ILogger<ImageLocalFileStorage> _logger;
        private readonly IWebHostEnvironment _webHostEnv;

        public ImageLocalFileStorage(
            ILogger<ImageLocalFileStorage> logger,
            IWebHostEnvironment webHostEnv)
        {
            _logger = logger;
            _webHostEnv = webHostEnv;
        }

        private string AbsoluteImageDirPath => Path.Combine(_webHostEnv.WebRootPath, imageDirectoryName);

        public Task DeleteImage(string uri)
        {
            if (uri == defaultProfilePictureName)
            {
                _logger.LogError($"Attempt to remove default profile picture failed.");
                return Task.CompletedTask;
            }

            try
            {
                File.Delete(uri);
                _logger.LogDebug($"Deleted image of type and path {uri}.");
                return Task.CompletedTask; ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError($"Failed to remove image with file path: ${uri}");
                return Task.CompletedTask; ;
            }
        }

        public async Task<string> UploadBlogCoverImageAsync(IFormFile imageFile)
        {
            var type = nameof(ImageType.BlogCover);
            return await UploadImageAsync(imageFile, type);
        }

        public async Task<string> UploadProfileImageAsync(IFormFile imageFile)
        {
            var type = nameof(ImageType.ProfilePicture);
            return await UploadImageAsync(imageFile, type);
        }

        private string BuildFileName(string originalName, string type)
        {
            return string.Join
            (
                "_",
                new string[]
                {
                DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                type,
                originalName.Trim('.','_','@', ' ', '#', '/', '\\', '!', '^', '&', '*'),
                }
            );
        }

        private async Task<string> UploadImageAsync(
            IFormFile imageFile,
            string type)
        {
            var pathRelativeToImageDir = Path.Combine(type);
            var directoryPath = Path.Combine(AbsoluteImageDirPath, pathRelativeToImageDir);
            var formattedName = BuildFileName(imageFile.FileName, type);
            Directory.CreateDirectory(directoryPath);
            var filePath = Path.Combine(directoryPath, formattedName);
            using var stream = File.Create(filePath);
            await imageFile.CopyToAsync(stream);
            _logger.LogInformation($"File path of uploaded image is {filePath}.");

            return Path.Combine(pathRelativeToImageDir, formattedName);
        }
    }
}