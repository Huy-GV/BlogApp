using System;

namespace RazorBlog.Data.Dtos;

public record BlogDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required DateTime CreationTime { get; set; }
    public required DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public required string AuthorName { get; set; }
    public required uint ViewCount { get; set; }
    public required string Introduction { get; set; }
    public required string CoverImageUri { get; set; }
}