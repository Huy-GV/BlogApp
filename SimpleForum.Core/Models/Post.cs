using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Models;

public abstract class Post<TId>
{
    public TId Id { get; set; } = default!;

    [Required]
    [MaxLength(2500)]
    public virtual string Body { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public DateTime LastUpdateTime { get; set; }

    public string AuthorUserName { get; set; } = string.Empty;

    public ApplicationUser AuthorUser { get; set; } = null!;

    public int? ReportTicketId { get; set; }

    public ReportTicket? ReportTicket { get; set; }

    public bool ToBeDeleted { get; set; } = false;
}
