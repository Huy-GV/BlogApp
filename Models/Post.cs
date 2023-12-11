using System;
using System.ComponentModel.DataAnnotations;

namespace RazorBlog.Models;

public class Post
{
    [Required]
    [MaxLength(2500)] 
    public virtual string Content { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public DateTime LastUpdateTime { get; set; }

    public string AppUserId { get; set; } = string.Empty;
    public ApplicationUser AppUser { get; set; } = null!;
    public bool IsHidden { get; set; }
    public bool ToBeDeleted { get; set; } = false;
}