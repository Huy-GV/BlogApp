namespace SimpleForum.Core.Data.Dtos;

public record CommentDto : PostDto
{
    public required string AuthorProfileImageUri { get; set; }
    public required string Content { get; set; }
}
