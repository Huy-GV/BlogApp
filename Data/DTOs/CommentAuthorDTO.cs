using System;

namespace BlogApp.Data.DTOs
{
    public class CommentAuthorDTO : BaseProfileDTO
    {
        public int CommentID { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }
}
