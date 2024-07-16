using System;

namespace SimpleForum.Core.Data.Dtos;

public record DetailedBlogDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string AuthorName { get; set; }
    public required string AuthorProfileImageUri { get; set; }
    public required string AuthorDescription { get; set; }
    public required string Introduction { get; set; }
    public required DateTime CreationTime { get; set; }
    public required DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public required string Content { get; set; }
    public required string CoverImageUri { get; set; }
    public bool IsHidden { get; set; }
}
