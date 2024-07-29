using System;

namespace SimpleForum.Core.Data.Dtos;

public record DetailedThreadDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string AuthorUserName { get; init; }
    public required string AuthorProfileImageUri { get; init; }
    public required string AuthorDescription { get; init; }
    public required string Introduction { get; init; }
    public required DateTime CreationTime { get; init; }
    public required DateTime LastUpdateTime { get; init; }
    public bool IsModified => CreationTime != LastUpdateTime;
    public required string Content { get; init; }
    public required string CoverImageUri { get; init; }
    public ReportTicketDto? ReportTicket { get; init; }
    public bool IsReported => ReportTicket != null;
}
