using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Data.ViewModels;

public class CommentViewModel
{
    [Required] 
    public int BlogId { get; set; }

    [StringLength(100, MinimumLength = 1)] 
    public string Content { get; set; } = string.Empty;
}