using System;

namespace SimpleForum.Core.Data.Dtos;

public abstract record ReportedPostDto
{
    public required int Id { get; init; }
    public required DateTime CreationTime { get; init; }
    public required ReportTicketDto ReportTicket { get; init; }
}
