using System.ComponentModel.DataAnnotations;

namespace BlogApp.Data.ViewModels
{
    public class CommentViewModel
    {
        [Required]
        [StringLength(100)]
        public string Content { get; set; }

        // public int? ParentCommentId { get; set; }
    }
}