using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data.Constants;

namespace RazorBlog.Core.Services;

internal class LocalImageStore : IImageStore
{
    private readonly ILogger<LocalImageStore> _logger;
    private readonly IWebHostEnvironment _webHostEnv;

    private const string ImageDirectoryName = "images";
    private const string ReadonlyDirectoryName = "readonly";
    private const string DefaultProfileImageName = "default.jpg";

    public LocalImageStore(
        ILogger<LocalImageStore> logger,
        IWebHostEnvironment webHostEnv)
    {
        _logger = logger;
        _webHostEnv = webHostEnv;
    }

    public Task<string> GetDefaultProfileImageUriAsync()
    {
        return Task.FromResult(Path.Combine(ImageDirectoryName, ReadonlyDirectoryName, DefaultProfileImageName));
    }

    public Task<ServiceResultCode> DeleteImage(string uri)
    {
        var trimmedUri = uri.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullImageFilePath = Path.Combine(_webHostEnv.WebRootPath, trimmedUri);
        _logger.LogInformation("Image uri '{uri}' expanded to '{fullImageFilePath}'", uri, fullImageFilePath);

        var directory = Directory.GetParent(fullImageFilePath)?.Name ?? string.Empty;
        if (directory.Equals(ReadonlyDirectoryName, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Failed to remove image at '{fullImageFilePath}' within readonly directory", fullImageFilePath);
            return Task.FromResult(ServiceResultCode.Unauthorized);
        }

        try
        {
            File.Delete(fullImageFilePath);
            _logger.LogDebug("Deleted image at {fullImageFilePath}", fullImageFilePath);
            return Task.FromResult(ServiceResultCode.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to remove image at {fullImageFilePath}: {ex}", fullImageFilePath, ex);
            return Task.FromResult(ServiceResultCode.Error);
        }
    }

    public async Task<(ServiceResultCode, string?)> UploadBlogCoverImageAsync(IFormFile imageFile)
    {
        try
        {
            var uri = await UploadImageAsync(imageFile, ImageType.BlogCover);
            return (ServiceResultCode.Success, uri);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to upload blog cover image named '{name}': {e}", imageFile.FileName, e);
            return (ServiceResultCode.Error, null);
        }
    }

    public async Task<(ServiceResultCode, string?)> UploadProfileImageAsync(IFormFile imageFile)
    {
        try
        {
            var uri = await UploadImageAsync(imageFile, ImageType.ProfileImage);
            return (ServiceResultCode.Success, uri);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to upload blog cover image named '{name}': {e}", imageFile.FileName, e);
            return (ServiceResultCode.Error, null);
        }
    }

    private static string BuildFileName(string originalName, string type)
    {
        return string.Join(
            "_",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            type,
            originalName
                .Trim(Path.GetInvalidFileNameChars()))
                .Replace(".", string.Empty)
                .Replace("_", string.Empty)
                .Replace("@", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("#", string.Empty)
                .Replace("/", string.Empty)
                .Replace("\\", string.Empty)
                .Replace("!", string.Empty)
                .Replace("^", string.Empty)
                .Replace("&", string.Empty)
                .Replace("*", string.Empty);
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
        _logger.LogInformation("File path of uploaded image is '{filePath}'", absoluteImageFilePath);

        // return the portion of the image path relative to the web content directory
        return Uri.EscapeDataString(relativeImageFilePath);
    }
}