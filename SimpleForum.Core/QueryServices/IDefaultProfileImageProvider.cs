namespace SimpleForum.Core.QueryServices;

public interface IDefaultProfileImageProvider
{
    /// <summary>
    /// Gets the default profile image URI.
    /// </summary>
    /// <returns>The URI of the default profile image.</returns>
    string GetDefaultProfileImageUri();
}
