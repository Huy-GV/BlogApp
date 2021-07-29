using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Blog : Post
    {
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        public ICollection<Comment> Comments { get; set; }

    }
}
