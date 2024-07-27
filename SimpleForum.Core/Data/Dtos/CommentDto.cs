using System;

namespace SimpleForum.Core.Data.Dtos;

public record CommentDto : PostDto
{
    public required string AuthorProfileImageUri { get; set; }
    public required string Content { get; set; }
    public int? ReportTicketId { get; set; }
    public string ReportingUserName { get; init; } = string.Empty;
    public DateTime? ReportDate { get; init; }
    public bool IsReported => ReportTicketId != null;
}
