using System.Collections.Generic;

namespace RazorBlog.Core.Data.Dtos;

public record PersonalProfileDto
{
    public required string UserName { get; init; }
    public string ProfileImageUri { get; init; } = "default";
    public required string RegistrationDate { get; init; }
    public string Description { get; init; } = "None";
    public required uint BlogCount { get; init; }
    public required uint BlogCountCurrentYear { get; init; }
    public required uint CommentCount { get; init; }
    public required uint ViewCountCurrentYear { get; init; } = 0;
    public Dictionary<uint, List<MinimalBlogDto>> BlogsGroupedByYear { get; init; } = new();
}