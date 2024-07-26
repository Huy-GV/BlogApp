using System.Collections.Generic;

namespace SimpleForum.Core.Data.Dtos;

public record PersonalProfileDto
{
    public string UserName { get; init; } = string.Empty;
    public string ProfileImageUri { get; init; } = "default";
    public string RegistrationDate { get; init; } = string.Empty;
    public string Description { get; init; } = "None";
    public uint ThreadCount { get; init; }
    public uint ThreadCountCurrentYear { get; init; }
    public uint CommentCount { get; init; }
    public uint ViewCountCurrentYear { get; init; } = 0;
    public Dictionary<uint, List<MinimalThreadDto>> ThreadsGroupedByYear { get; init; } = [];
}
