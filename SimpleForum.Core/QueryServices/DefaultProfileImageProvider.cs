using Microsoft.Extensions.Options;
using SimpleForum.Core.Options;
using System.IO;

namespace SimpleForum.Core.QueryServices;
internal class DefaultProfileImageProvider : IDefaultProfileImageProvider
{
    private readonly string _imageDirectoryName;
    private readonly string _readonlyDirectoryName;
    private readonly string _defaultProfileImageName;
    public DefaultProfileImageProvider(IOptions<DefaultProfileImageOptions> options)
    {
        _imageDirectoryName = options.Value.ImageDirectoryName;
        _readonlyDirectoryName = options.Value.ReadonlyDirectoryName;
        _defaultProfileImageName = options.Value.DefaultProfileImageName;
    }
    public string GetDefaultProfileImageUri()
    {
        return Path.Combine(_imageDirectoryName, _readonlyDirectoryName, _defaultProfileImageName);
    }
}
