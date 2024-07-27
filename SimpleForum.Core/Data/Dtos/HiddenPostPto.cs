using System;

namespace SimpleForum.Core.Data.Dtos;

public record HiddenPostPto
{
    public required int Id { get; init; }
    public required DateTime CreationTime { get; init; }
    public required string ReportingUserName { get; init; }
    public required DateTime ReportDate { get; init; }
    public required int ReportTicketId { get; init; }
}
