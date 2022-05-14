using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Models;

public class Blog : Post
{
    public int Id { get; set; }

    [Required] public string Title { get; set; }

    public string Introduction { get; set; } = string.Empty;
    public uint ViewCount { get; set; } = 0;

    [Required] public string CoverImageUri { get; set; }

    public ICollection<Comment> Comments { get; set; }
}