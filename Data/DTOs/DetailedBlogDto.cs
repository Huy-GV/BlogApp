using System;
using System.Collections.Generic;

namespace RazorBlog.Data.Dtos;

public record DetailedBlogDto
{
    public int Id { get; set; }
    public string Title { get; set; }

    public string AuthorName { get; set; }
    public string AuthorProfileImageUri { get; set; }
    public string AuthorDescription { get; set; }
    public string Introduction { get; set; }

    public string Content { get; set; }
    public DateTime Date { get; set; }
    public string CoverImageUri { get; set; }
    public bool IsHidden { get; set; }
    public ICollection<CommentDto> CommentDtos { get; set; }
}