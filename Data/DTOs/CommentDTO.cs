using System;

namespace BlogApp.Data.DTOs
{
    public class CommentDTO
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfilePicture { get; set; }
        public bool IsHidden { get; set; }
    }
}
