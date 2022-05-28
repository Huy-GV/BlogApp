namespace RazorBlog.Data.DTOs;

public record CurrentUserInfo
{
    public bool AllowedToModifyOrDeleteBlog { get; init; }
    public bool AllowedToHideBlogOrComment { get; init; }
    public bool IsBanned { get; init; }
    public bool IsAuthenticated { get; init; }
    public string UserName { get; init; }
}