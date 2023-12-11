using System;

namespace RazorBlog.Data.Dtos;

public record BlogDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public string AuthorName { get; set; } = string.Empty;
    public uint ViewCount { get; set; }
    public string Introduction { get; set; } = string.Empty;
    public string CoverImageUri { get; set; } = string.Empty;
}