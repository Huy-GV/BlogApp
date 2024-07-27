namespace SimpleForum.Core.Data.Dtos;

public record IndexThreadDto : PostDto
{
    public required string Title { get; set; }
    public required uint ViewCount { get; set; }
    public required string Introduction { get; set; }
    public required string CoverImageUri { get; set; }
}
