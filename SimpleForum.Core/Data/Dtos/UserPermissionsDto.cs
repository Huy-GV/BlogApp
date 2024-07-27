namespace SimpleForum.Core.Data.Dtos;

public record UserPermissionsDto
{
    public bool AllowedToModifyOrDeletePost { get; init; }
    public bool AllowedToCreateComment { get; init; }
    public bool AllowedToReportPost { get; init; }
    public bool IsAuthenticated { get; init; }
    public required string UserName { get; init; }
}
