using System;

namespace RazorBlog.Data.Dtos;

public record BlogDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public string? AuthorName { get; set; }
    public uint ViewCount { get; set; }
    public string? Introduction { get; set; }
    public string? CoverImageUri { get; set; }
}