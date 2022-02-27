using System;

namespace BlogApp.Data.DTOs
{
    public class CommentAuthorDto : BaseProfileDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }
}
