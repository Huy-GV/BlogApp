using System;

namespace SimpleForum.Core.Data.Dtos;

public record ThreadHistoryEntryDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required DateTime CreationTime { get; init; }
    public required DateTime LastUpdateTime { get; init; }
    public required uint ViewCount { get; init; }
}
