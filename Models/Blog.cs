using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Blog : Post
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Introduction { get; set; } = string.Empty;
        public uint ViewCount { get; set; } = 0;

        [Required]
        public string CoverImageUri { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}