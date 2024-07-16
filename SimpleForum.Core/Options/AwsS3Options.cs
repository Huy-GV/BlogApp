using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Options;

public class AwsOptions
{
    public const string SectionName = "Aws";

    [Required]
    public required string DataBucket { get; init; }

    [Required]
    public required string Profile { get; init; }

    [Required]
    public required string Region { get; init; }

}
