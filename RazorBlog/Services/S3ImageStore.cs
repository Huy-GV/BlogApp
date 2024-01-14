using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
using System.Threading.Tasks;

namespace RazorBlog.Services;

public class S3ImageStore : IImageStore
{
    private readonly ILogger<S3ImageStore> _logger;
    private readonly IWebHostEnvironment _webHostEnv;

    public S3ImageStore(ILogger<S3ImageStore> logger, IWebHostEnvironment webHostEnv)
    {
        _logger = logger;
        _webHostEnv = webHostEnv;
    }

    public Task<ServiceResultCode> DeleteImage(string uri)
    {
        throw new System.NotImplementedException();
    }

    public Task<string> GetDefaultProfileImageUriAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<(ServiceResultCode, string?)> UploadBlogCoverImageAsync(IFormFile imageFile)
    {
        throw new System.NotImplementedException();
    }

    public Task<(ServiceResultCode, string?)> UploadProfileImageAsync(IFormFile imageFile)
    {
        throw new System.NotImplementedException();
    }
}
