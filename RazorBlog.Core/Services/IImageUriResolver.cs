using System.Threading.Tasks;
using RazorBlog.Core.Communication;

namespace RazorBlog.Core.Services;

public interface IImageUriResolver
{
    /// <summary>
    /// Resolves the provided image URI asynchronously.
    /// </summary>
    /// <param name="imageUri">The image URI to be resolved.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a tuple with
    /// a <see cref="ServiceResultCode"/> indicating the result of the operation,
    /// and a string representing the resolved image URI (null if not resolved).
    /// </returns>
    Task<(ServiceResultCode, string?)> ResolveImageUri(string imageUri);
}
