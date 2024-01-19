using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;

namespace RazorBlog.Services;

public class LocalImageUriResolver : IImageUriResolver
{
    private readonly ILogger<LocalImageUriResolver> _logger;

    public LocalImageUriResolver(ILogger<LocalImageUriResolver> logger)
    {
        _logger = logger;
    }
    
    public Task<(ServiceResultCode, string?)> ResolveImageUri(string imageUri)
    {
        if (Uri.IsWellFormedUriString(imageUri, UriKind.RelativeOrAbsolute))
        {
            return Task.FromResult((ServiceResultCode.Success, imageUri))!;
        }
        
        _logger.LogError("Failed to resolve image uri '{uri}' because it is not well-formed", imageUri);

        var escapedUri = Uri.EscapeDataString(imageUri);
        _logger.LogInformation("Retrying with escaped uri '{escapedUri}'", escapedUri);
        if (Uri.IsWellFormedUriString(escapedUri, UriKind.RelativeOrAbsolute))
        {
            return Task.FromResult((ServiceResultCode.InvalidArguments, (string?)null));
        }

        _logger.LogError("Retry failed because '{escapedUri}' is not well-formed", imageUri);
        return Task.FromResult((ServiceResultCode.Success, imageUri))!;

    }
}