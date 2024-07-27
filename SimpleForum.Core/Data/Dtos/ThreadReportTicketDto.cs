using System;

namespace SimpleForum.Core.Data.Dtos;
public class ThreadReportTicketDto
{
    public required string ReportingUserName { get; set; }
    public required DateTime ReportDate { get; set; }
}
