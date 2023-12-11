using System;

namespace RazorBlog.Data.Dtos;

public record CommentDto
{
    public required int Id { get; set; }
    // TODO: extract to parent class
    public required DateTime CreationTime { get; set; }
    public required DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public required string Content { get; set; }
    public required string AuthorName { get; set; }
    public required string AuthorProfileImageUri { get; set; }
    public bool IsHidden { get; set; }
}