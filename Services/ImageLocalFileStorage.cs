using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RazorBlog.Data.Constants;

namespace RazorBlog.Services;

public class ImageLocalFileStorage(
    ILogger<ImageLocalFileStorage> logger,
    IWebHostEnvironment webHostEnv) : IImageStorage
{
    private readonly ILogger<ImageLocalFileStorage> _logger = logger;
    private readonly IWebHostEnvironment _webHostEnv = webHostEnv;
    private const string DefaultProfilePictureName = "default.jpg";
    private const string ImageDirectoryName = "images";

    private string AbsoluteImageDirPath => Path.Combine(_webHostEnv.WebRootPath, ImageDirectoryName);

    public Task DeleteImage(string uri)
    {
        if (uri == DefaultProfilePictureName)
        {
            _logger.LogError("Attempt to remove default profile picture failed.");
            return Task.CompletedTask;
        }

        try
        {
            File.Delete(uri);
            _logger.LogDebug($"Deleted image of type and path {uri}.");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to remove image with file path: ${uri}");
            _logger.LogError(ex.Message);

            return Task.CompletedTask;
        }
    }

    public async Task<string> UploadBlogCoverImageAsync(IFormFile imageFile)
    {
        return await UploadImageAsync(imageFile, ImageType.BlogCover);
    }

    public async Task<string> UploadProfileImageAsync(IFormFile imageFile)
    {
        return await UploadImageAsync(imageFile, ImageType.ProfileImage);
    }

    private static string BuildFileName(string originalName, string type)
    {
        return string.Join
        (
        "_", DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), type,
        originalName.Trim('.', '_', '@', ' ', '#', '/', '\\', '!', '^', '&', '*'));
    }

    private async Task<string> UploadImageAsync(
        IFormFile imageFile,
        ImageType type)
    {
        var pathRelativeToImageDir = Path.Combine(Enum.GetName(type)!);
        var directoryPath = Path.Combine(AbsoluteImageDirPath, pathRelativeToImageDir);
        var formattedName = BuildFileName(imageFile.FileName, nameof(type));
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, formattedName);
        await using var stream = File.Create(filePath);
        await imageFile.CopyToAsync(stream);
        _logger.LogInformation($"File path of uploaded image is {filePath}.");

        return Path.Combine(pathRelativeToImageDir, formattedName);
    }
}