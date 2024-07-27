using System;

namespace SimpleForum.Core.Data.Dtos;

public record MinimalThreadDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required DateTime CreationTime { get; set; }
    public required uint ViewCount { get; set; }
}
