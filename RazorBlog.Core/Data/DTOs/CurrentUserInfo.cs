namespace RazorBlog.Core.Data.Dtos;

public record CurrentUserInfo
{
    public bool AllowedToModifyOrDeletePost { get; init; }
    public bool AllowedToCreateComment { get; init; }
    public bool AllowedToHidePost { get; init; }
    public bool IsAuthenticated { get; init; }
    public required string UserName { get; init; }
}