using System;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Core.Models;

public class BanTicket
{
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    public DateTime? Expiry { get; set; }

    public ApplicationUser AppUser { get; set; } = null!;
}