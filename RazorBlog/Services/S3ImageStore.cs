using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RazorBlog.Communication;
using RazorBlog.Data.Constants;
using RazorBlog.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RazorBlog.Services;

public class S3ImageStore : IImageStore
{
    private readonly ILogger<S3ImageStore> _logger;
    private readonly IWebHostEnvironment _webHostEnv;
    private readonly IAmazonS3 _awsS3Client;
    private readonly AwsS3Options _awsS3Options;

    public S3ImageStore(
        ILogger<S3ImageStore> logger,
        IWebHostEnvironment webHostEnv,
        IAmazonS3 awsS3Client,
        IOptions<AwsS3Options> awsS3Options)
    {
        _logger = logger;
        _webHostEnv = webHostEnv;
        _awsS3Client = awsS3Client;
        _awsS3Options = awsS3Options.Value;
    }

    public async Task<ServiceResultCode> DeleteImage(string uri)
    {
        var request = new DeleteObjectRequest
        {
            Key = uri,
            BucketName = _awsS3Options.BucketName,
        };

        _logger.LogInformation("Deleting image with uri '{uri}'", uri);

        try
        {
            var deleteObjectResponse = await _awsS3Client.DeleteObjectAsync(request);
            if (deleteObjectResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                return ServiceResultCode.Success;
            }

            _logger.LogError(
                "Failed to delete image, HTTP code: {code}",
                deleteObjectResponse.HttpStatusCode);
            return ServiceResultCode.Error;
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {ex}", ex);
            return ServiceResultCode.Error;
        }
    }

    public Task<string> GetDefaultProfileImageUriAsync()
    {
        throw new NotImplementedException();
    }

    private static string BuildFileName(string originalName, string type)
    {
        return string.Join(
            "_",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            type,
            originalName.Trim('.', '_', '@', ' ', '#', '/', '\\', '!', '^', '&', '*'));
    }

    public async Task<(ServiceResultCode, string?)> UploadBlogCoverImageAsync(IFormFile imageFile)
    {
        var objectName = Guid.NewGuid().ToString();

        var tag = new Tag()
        {
            Key = nameof(ImageType),
            Value = ImageType.BlogCover.ToString(),
        };

        var request = new PutObjectRequest
        {
            Key = BuildFileName(imageFile.FileName, ImageType.BlogCover.ToString()),
            BucketName = _awsS3Options.BucketName,
            InputStream = imageFile.OpenReadStream(),
            TagSet = [tag],
        };

        _logger.LogInformation(
            "Uploading blog cover image '{fileName}' to bucket '{bucketName}'",
            request.Key,
            request.BucketName);

        try
        {
            var putObjectResponse = await _awsS3Client.PutObjectAsync(request);
            if (putObjectResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                return (ServiceResultCode.Success, request.Key);
            }

            _logger.LogError(
                "Failed to upload blog cover image, HTTP code: {code}",
                putObjectResponse.HttpStatusCode);
            return (ServiceResultCode.Error, null);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {ex}", ex);
            return (ServiceResultCode.Error, null);
        }
    }

    public async Task<(ServiceResultCode, string?)> UploadProfileImageAsync(IFormFile imageFile)
    {
        var objectName = Guid.NewGuid().ToString();

        var tag = new Tag()
        {
            Key = nameof(ImageType),
            Value = ImageType.ProfileImage.ToString(),
        };

        var request = new PutObjectRequest
        {
            Key = BuildFileName(imageFile.FileName, ImageType.ProfileImage.ToString()),
            BucketName = _awsS3Options.BucketName,
            InputStream = imageFile.OpenReadStream(),
            TagSet = [tag],
        };

        _logger.LogInformation(
            "Uploading profile image '{fileName}' to bucket '{bucketName}'",
            request.Key,
            request.BucketName);

        try
        {
            var putObjectResponse = await _awsS3Client.PutObjectAsync(request);
            if (putObjectResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                return (ServiceResultCode.Success, request.Key);
            }

            _logger.LogError(
                "Failed to upload profile image, HTTP code: {code}",
                putObjectResponse.HttpStatusCode);
            return (ServiceResultCode.Error, null);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {ex}", ex);
            return (ServiceResultCode.Error, null);
        }
    }
}
