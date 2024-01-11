using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class CreateBlogViewModel : BlogViewModel
{
    [Display(Name = "Cover image")]
    [Required]
    public IFormFile CoverImage { get; set; } = null!;
}