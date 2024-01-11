using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RazorBlog.Data.Constants;

namespace RazorBlog.Services;

public class ImageLocalFileStorage : IImageStorage
{
    private readonly ILogger<ImageLocalFileStorage> _logger;
    private readonly IWebHostEnvironment _webHostEnv;
    
    private const string DefaultProfilePictureName = "default.jpg";
    private const string ImageDirectoryName = "images";
    private const string ReadonlyDirectoryName = "readonly";
    private const string DefaultProfileImageName = "default.jpg";
    
    public ImageLocalFileStorage(
        ILogger<ImageLocalFileStorage> logger,
        IWebHostEnvironment webHostEnv)
    {
        _logger = logger;
        _webHostEnv = webHostEnv;
    }

    public Task<string> GetDefaultProfileImageUriAsync()
    {
        return Task.FromResult(Path.Combine(ImageDirectoryName, ReadonlyDirectoryName, DefaultProfileImageName));
    }

    public Task DeleteImage(string uri)
    {
        var trimmedUri = uri.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullImageFilePath = Path.Combine(_webHostEnv.WebRootPath, trimmedUri);
        _logger.LogInformation("Image uri '{uri}' expanded to '{fullImageFilePath}'", uri, fullImageFilePath);
        
        var directory = Directory.GetParent(fullImageFilePath)?.Name ?? string.Empty;
        
        if (directory.Equals(ReadonlyDirectoryName, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Failed to remove image at '{fullImageFilePath}' within readonly directory", fullImageFilePath);
            return Task.CompletedTask;
        }

        try
        {
            File.Delete(fullImageFilePath);
            _logger.LogDebug("Deleted image at {fullImageFilePath}", fullImageFilePath);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to remove image at {fullImageFilePath}: {ex}", fullImageFilePath, ex);
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
        return string.Join(
            "_", 
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
            type,
            originalName.Trim('.', '_', '@', ' ', '#', '/', '\\', '!', '^', '&', '*'));
    }

    private async Task<string> UploadImageAsync(
        IFormFile imageFile,
        ImageType type)
    {
        var imageTypeName = Enum.GetName(type)!;
        
        // ensure the directory for the image type exists
        var directoryPath = Path.Combine(ImageDirectoryName, imageTypeName);
        Directory.CreateDirectory(directoryPath);
        
        // creates a new image name
        var formattedName = BuildFileName(imageFile.FileName, imageTypeName);
        var relativeImageFilePath = Path.Combine(directoryPath, formattedName);
        var absoluteImageFilePath = Path.Combine(_webHostEnv.WebRootPath, relativeImageFilePath);
        
        await using var stream = File.Create(absoluteImageFilePath);
        await imageFile.CopyToAsync(stream);
        _logger.LogInformation("File path of uploaded image is {filePath}", absoluteImageFilePath);
        
        // return the portion of the image path relative to the web content directory
        return relativeImageFilePath;
    }
}