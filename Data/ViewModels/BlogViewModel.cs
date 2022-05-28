using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RazorBlog.Data.Validation;

namespace RazorBlog.Data.ViewModels;

public class BlogViewModel
{
    [Required]
    [StringLength(60, MinimumLength = 10)]
    public string Title { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 200)]
    public string Content { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Introduction { get; set; }

    [Required]
    [FileType(".jpg", ".jpeg", ".png", ErrorMessage = "Only jpg/jpeg and png files are allowed")]
    [DisplayName("Cover Image (jpg/ png)")]
    public IFormFile CoverImage { get; set; }
}