using System;

namespace RazorBlog.Data.DTOs;

public class CommentAuthorDto : BaseProfileDto
{
    public int CommentId { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
}