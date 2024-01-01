using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Models;

public class Comment : Post
{
    public int Id { get; set; }

    [Required] 
    [MaxLength(250)] 
    public override string Body { get; set; } = string.Empty;

    public int BlogId { get; set; }
}