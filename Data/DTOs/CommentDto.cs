using System;

namespace RazorBlog.Data.Dtos;

public record CommentDto
{
    public int Id { get; set; }
    // TODO: extract to parent class
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorProfileImageUri { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
}