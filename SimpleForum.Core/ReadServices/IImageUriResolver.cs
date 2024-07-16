using System.Threading.Tasks;
using SimpleForum.Core.Communication;

namespace SimpleForum.Core.ReadServices;

public interface IImageUriResolver
{
    /// <summary>
    /// Resolves the provided image URI asynchronously.
    /// </summary>
    /// <param name="imageUri">The image URI to be resolved.</param>
    /// <returns>
    /// The task result contains a tuple with
    /// a <see cref="ServiceResultCode"/> indicating the result of the operation,
    /// and a string representing the resolved image URI (null if not resolved).
    /// </returns>
    Task<(ServiceResultCode, string?)> ResolveImageUri(string imageUri);
}
