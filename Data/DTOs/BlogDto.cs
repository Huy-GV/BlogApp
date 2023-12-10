using System;

namespace RazorBlog.Data.Dtos;

public record BlogDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AuthorName { get; set; }
    public uint ViewCount { get; set; }
    public string Introduction { get; set; }
    public DateTime Date { get; set; }
    public string CoverImageUri { get; set; }
}