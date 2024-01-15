using System.Threading.Tasks;

namespace RazorBlog.Services;

public interface IHaveDefaultProfileImage
{
    /// <summary>
    /// Gets the default profile image URI.
    /// </summary>
    /// <returns>The URI of the default profile image.</returns>
    Task<string> GetDefaultProfileImageUriAsync();
}
