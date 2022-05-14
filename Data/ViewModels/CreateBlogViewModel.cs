using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class CreateBlogViewModel
{
    [Required]
    [StringLength(60, MinimumLength = 10)]
    public string Title { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 200)]
    public string Content { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Description { get; set; }

    [Display(Name = "Cover image")]
    [Required]
    public IFormFile CoverImage { get; set; }
}