namespace SimpleForum.Core.Data.Dtos;

public record CommentDto : PostDto
{
    public required string AuthorProfileImageUri { get; init; }
    public required string Content { get; init; }
    public required bool IsDeleted { get; set; }
    public ReportTicketDto? ReportTicketDto { get; init; }
    public bool IsReported => ReportTicketDto != null;
}
