using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
using RazorBlog.Utils;

namespace RazorBlog.Services;

public class S3ImageUriResolver : IImageUriResolver
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3ImageUriResolver> _logger;

    public S3ImageUriResolver(IAmazonS3 s3Client, ILogger<S3ImageUriResolver> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<(ServiceResultCode, string?)> ResolveImageUri(string imageUri)
    {
        if (string.IsNullOrEmpty(imageUri))
        {
            _logger.LogError("Failed to resolve empty image uri");
            return (ServiceResultCode.InvalidArguments, null);
        }
        
        if (!AwsUtils.TryConvertToS3Uri(imageUri, out var s3Uri))
        {
            _logger.LogError("Failed to convert uri '{uri}' to AWS S3 uri", imageUri);
            return (ServiceResultCode.InvalidArguments, null);
        }
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = s3Uri.Bucket,
            Key = s3Uri.Key,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        var preSignedUrl = await _s3Client.GetPreSignedURLAsync(request);
        return (ServiceResultCode.Success, preSignedUrl);
    }
}