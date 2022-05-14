using System.ComponentModel.DataAnnotations;

namespace BlogApp.Data.ViewModels
{
    public class CommentViewModel
    {
        [Required]
        public int BlogId { get; set; }
        
        [StringLength(100, MinimumLength = 1)]
        public string Content { get; set; }

        // public int? ParentCommentId { get; set; }
    }
}