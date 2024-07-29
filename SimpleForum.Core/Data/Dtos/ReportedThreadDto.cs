namespace SimpleForum.Core.Data.Dtos;

public record ReportedThreadDto : ReportedPostDto
{
    public required string Title { get; init; }
}
