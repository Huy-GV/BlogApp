using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Models;

public class Blog : Post<int>
{
    [Required] 
    public string Title { get; set; } = string.Empty;

    public string Introduction { get; set; } = string.Empty;

    public uint ViewCount { get; set; } = 0;

    [Required] 
    public string CoverImageUri { get; set; } = string.Empty;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}