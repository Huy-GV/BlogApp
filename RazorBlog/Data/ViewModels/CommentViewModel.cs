using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Data.ViewModels;

public class CommentViewModel
{
    [Required] 
    public int BlogId { get; set; }

    [StringLength(200, MinimumLength = 1)] 
    public string Body { get; set; } = string.Empty;
}