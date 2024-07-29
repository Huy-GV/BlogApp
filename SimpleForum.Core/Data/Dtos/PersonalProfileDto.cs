using System.Collections.Generic;

namespace SimpleForum.Core.Data.Dtos;

public record PersonalProfileDto
{
    public required string UserName { get; init; } = string.Empty;
    public required string ProfileImageUri { get; init; } = "default";
    public required string RegistrationDate { get; init; } = string.Empty;
    public required string Description { get; init; } = "None";
    public required uint ThreadCount { get; init; }
    public required uint ThreadCountCurrentYear { get; init; }
    public required uint CommentCount { get; init; }
    public required uint ViewCountCurrentYear { get; init; } = 0;
    public required IReadOnlyDictionary<uint, IReadOnlyCollection<ThreadHistoryEntryDto>> ThreadsGroupedByYear { get; init; }
}
