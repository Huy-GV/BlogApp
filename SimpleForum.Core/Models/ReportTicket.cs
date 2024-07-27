using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Models;

public class ReportTicket
{
    public int Id { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreationDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ActionDate { get; set; }

    public int? ThreadId { get; set; }

    public int? CommentId { get; set; }

    public string ReportingUserName { get; set; } = string.Empty;

    public ApplicationUser ReportingUser { get; set; } = null!;
}
