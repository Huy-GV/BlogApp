using System.Collections.Generic;

namespace RazorBlog.Data.Dtos;

public record PersonalProfileDto
{
    public string UserName { get; init; } = string.Empty;
    public string ProfileImageUri { get; init; } = "default";
    public string RegistrationDate { get; init; } = string.Empty;
    public string Description { get; init; } = "None";
    public uint BlogCount { get; init; }
    public uint BlogCountCurrentYear { get; init; }
    public uint CommentCount { get; init; }
    public uint ViewCountCurrentYear { get; init; } = 0;
    public Dictionary<uint, List<MinimalBlogDto>> BlogsGroupedByYear { get; init; } = new();
}