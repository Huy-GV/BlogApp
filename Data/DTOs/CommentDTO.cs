using System;

namespace RazorBlog.Data.DTOs;

public record CommentDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; }
    public string AuthorName { get; set; }
    public string AuthorProfileImageUri { get; set; }
    public bool IsHidden { get; set; }
}