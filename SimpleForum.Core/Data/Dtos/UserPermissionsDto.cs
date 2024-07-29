namespace SimpleForum.Core.Data.Dtos;

public record UserPermissionsDto
{
    public required bool AllowedToModifyOrDeletePost { get; init; }
    public required bool AllowedToCreateComment { get; init; }
    public required bool AllowedToReportPost { get; init; }
    public required string UserName { get; init; }
}
