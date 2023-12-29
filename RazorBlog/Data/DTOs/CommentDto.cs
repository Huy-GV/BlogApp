using RazorBlog.Data.DTOs;

namespace RazorBlog.Data.Dtos;

public record CommentDto : PostPto
{
    public required string AuthorProfileImageUri { get; set; }
    public required string Content { get; set; }
}