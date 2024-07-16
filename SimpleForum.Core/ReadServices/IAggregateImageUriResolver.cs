using System.Threading.Tasks;

namespace SimpleForum.Core.ReadServices;

public interface IAggregateImageUriResolver
{
    /// <summary>
    /// Resolve the image URI using registered resolvers.
    /// </summary>
    /// <param name="imageUri">Original image URI.</param>
    /// <returns>Resolved image uri if successful; otherwise, null</returns>
    public Task<string?> ResolveImageUriAsync(string imageUri);
}
