namespace SimpleForum.Core.Data.Dtos;

public record CommentDto : PostPto
{
    public required string AuthorProfileImageUri { get; set; }
    public required string Content { get; set; }
}
