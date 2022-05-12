using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BlogApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [DataType(DataType.Date)]
        public DateTime? RegistrationDate { get; set; }

        public string Description { get; set; }
        public string ProfileImageUri { get; set; }
        public ICollection<Blog> Blogs { get; set; }
        public ICollection<Comment> Comments { get; set; }

        [NotMapped]
        public override bool LockoutEnabled { get; set; }

        [NotMapped]
        public override int AccessFailedCount { get; set; }

        [NotMapped]
        public override string? PhoneNumber { get; set; }

        [NotMapped]
        public override string? SecurityStamp { get; set; }

        [NotMapped]
        public override DateTimeOffset? LockoutEnd { get; set; }

        [NotMapped]
        public override bool TwoFactorEnabled { get; set; }

        [NotMapped]
        public override bool PhoneNumberConfirmed { get; set; }
    }
}