using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Options;
internal class DefaultProfileImageOptions
{
    public const string SectionName = "DefaultProfileImage";

    [Required]
    public required string ImageDirectoryName { get; init; }
    [Required]
    public required string ReadonlyDirectoryName { get; init; }
    [Required]
    public required string DefaultProfileImageName { get; init; }
}
