namespace SimpleForum.Core.Data.Dtos;

public record ReportedCommentDto : ReportedPostDto
{
    public required int ThreadId { get; init; }
}
