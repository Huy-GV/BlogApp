using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Models;

public class Comment : Post<int>
{
    [Required]
    [MaxLength(250)]
    public override string Body { get; set; } = string.Empty;

    public int ThreadId { get; set; }
}
