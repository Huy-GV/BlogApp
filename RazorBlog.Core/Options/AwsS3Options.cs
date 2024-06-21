using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Core.Options;

public class AwsOptions
{
    public const string Name = "Aws";

    [Required]
    public required string DataBucket { get; init; }
    
    [Required]
    public required string Profile { get; init; }

    [Required]
    public required string Region { get; init; }

}
