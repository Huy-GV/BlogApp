namespace SimpleForum.Core.Data.Dtos;

public record IndexThreadDto : PostDto
{
    public required string Title { get; init; }
    //public required uint ViewCount { get; set; }
    public required string Introduction { get; init; }
    public required string CoverImageUri { get; init; }
}
