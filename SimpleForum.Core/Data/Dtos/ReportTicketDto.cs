using System;

namespace SimpleForum.Core.Data.Dtos;
public class ReportTicketDto
{
    public required int Id { get; init; }
    public required string ReportingUserName { get; init; }
    public required DateTime ReportDate { get; init; }
}
