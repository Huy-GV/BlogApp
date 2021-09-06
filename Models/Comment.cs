using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Comment : Post
    {
        public int ID { get; set; }
        [Required, MaxLength(250)]
        public new string Content { get; set; }
        [Required]
        public int BlogID { get; set; }
    }
}
