using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Data.DTOs
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string JoinDate { get; set; }
        public bool IsModerator { get; set; } = false;
        public int BlogCount { get; set; }
        public int CommentCount { get; set; }
    }
}
