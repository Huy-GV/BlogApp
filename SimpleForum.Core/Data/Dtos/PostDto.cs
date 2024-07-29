using System;

namespace SimpleForum.Core.Data.Dtos;

public abstract record PostDto
{
    public required int Id { get; set; }
    public required DateTime CreationTime { get; set; }
    public required DateTime LastUpdateTime { get; set; }
    public required string AuthorName { get; set; }
    public bool IsModified => CreationTime != LastUpdateTime;
}
