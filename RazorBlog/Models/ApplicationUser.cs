using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RazorBlog.Models;

public class ApplicationUser : IdentityUser
{
    [DataType(DataType.Date)] 
    public DateTime? RegistrationDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ProfileImageUri { get; set; } = string.Empty;

    [NotMapped] public override bool LockoutEnabled { get; set; }

    [NotMapped] public override int AccessFailedCount { get; set; }

    [NotMapped] public override string? PhoneNumber { get; set; }

    [NotMapped] public override string? SecurityStamp { get; set; }

    [NotMapped] public override DateTimeOffset? LockoutEnd { get; set; }

    [NotMapped] public override bool TwoFactorEnabled { get; set; }

    [NotMapped] public override bool PhoneNumberConfirmed { get; set; }
}