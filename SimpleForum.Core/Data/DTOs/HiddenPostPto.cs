using System;

namespace SimpleForum.Core.Data.Dtos;

public record HiddenPostPto
{
    public required int Id { get; init; }
    public required string Content { get; init; }
    public required DateTime CreationTime { get; init; }
}
