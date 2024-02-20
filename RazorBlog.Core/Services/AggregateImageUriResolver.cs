using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Communication;

namespace RazorBlog.Core.Services;

internal class AggregateImageUriResolver : IAggregateImageUriResolver
{
    private readonly IReadOnlyCollection<IImageUriResolver> _imageUriResolvers;
    private readonly ILogger<AggregateImageUriResolver> _logger;

    public AggregateImageUriResolver(
        ILogger<AggregateImageUriResolver> logger,
        IEnumerable<IImageUriResolver> imageUriResolvers)
    {
        _logger = logger;
        _imageUriResolvers = imageUriResolvers.ToImmutableList();
    }

    public async Task<string?> ResolveImageUriAsync(string imageUri)
    {
        _logger.LogInformation("Resolving image uri '{imageUri}'", imageUri);
        foreach (var imageResolver in _imageUriResolvers)
        {
            if (string.IsNullOrEmpty(imageUri))
            {
                _logger.LogError("Failed to resolve empty image uri");
                return null;
            }

            _logger.LogInformation("Using resolver '{imageResolver}'", imageResolver.GetType().FullName);
            var (result, uri) = await imageResolver.ResolveImageUri(imageUri);
            if (result != ServiceResultCode.Success)
            {
                continue;
            }

            _logger.LogInformation("Image uri '{imageUri}' resolved to '{uri}'", imageUri, uri);
            return uri;
        }

        return null;
    }
}
