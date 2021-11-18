﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlogApp.Models;
using System.Threading.Tasks;

namespace BlogApp.Data.DTOs
{
    public class UserDTO : SimpleProfileDTO
    {

        public string RegistrationDate { get; set; }
        public bool IsModerator { get; set; } = false;
        public int BlogCount { get; set; }
        public int BlogCountCurrentYear { get; set; }
        public int CommentCount { get; set; }
        public int ViewCountCurrentYear { get; set; } = 0;
        public List<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
