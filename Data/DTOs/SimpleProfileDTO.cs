using System;
using System.Collections.Generic;
using System.Linq;
using BlogApp.Models;
using System.Threading.Tasks;

namespace BlogApp.Data.DTOs
{
    public class SimpleProfileDTO
    {
        public string Username { get; set; }
        public string Country { get; set; } 
        public string Description { get; set; } = "None";
        public string ProfilePath { get; set; } = "default";
    }
}
