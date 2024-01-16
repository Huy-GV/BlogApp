using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RazorBlog.Data.Validation;

namespace RazorBlog.Data.ViewModels;

public class CreateBlogViewModel : BlogViewModel
{
    [Display(Name = "Cover image")]
    [Required]
    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile CoverImage { get; set; } = null!;
}