using System;

namespace BlogApp.Data.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string AuthorProfilePicturePath { get; set; }
        public bool IsHidden { get; set; }
    }
}
