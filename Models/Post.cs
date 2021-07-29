﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Post
    {
        public string? UserID { get; set; }
        [Required, MaxLength(255)]
        public string Content { get; set; }
        [DataType(DataType.Date), Required]
        public DateTime Date { get; set; }
        public string Author { get; set; }
    }
}
