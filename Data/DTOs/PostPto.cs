using System;

namespace RazorBlog.Data.DTOs;

public record PostPto
{
    public required int Id { get; set; }
    public required DateTime CreationTime { get; set; }
    public required DateTime LastUpdateTime { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public required string AuthorName { get; set; }
    public bool IsHidden { get; set; }
}
