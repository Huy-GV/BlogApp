using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Data
{
    public class Roles
    {
        public static readonly string AdminRole = "admin";
        public static readonly string ModeratorRole = "moderator";
        public static readonly string UserRole = "user";
    }
    public class Messages
    {
        public static readonly string InappropriateBlog = "The blog has been hidden due to inappropriate content. The admin will decide whether it gets removed or not";
        public static readonly string InappropriateComment = "The comment has been hidden due to inappropriate content. The admin will decide whether it gets removed or not";
    }

}
