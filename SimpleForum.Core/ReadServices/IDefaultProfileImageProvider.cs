using System.Threading.Tasks;

namespace SimpleForum.Core.ReadServices;

public interface IDefaultProfileImageProvider
{
    /// <summary>
    /// Gets the default profile image URI.
    /// </summary>
    /// <returns>The URI of the default profile image.</returns>
    Task<string> GetDefaultProfileImageUriAsync();
}
