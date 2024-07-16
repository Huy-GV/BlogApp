using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SimpleForum.Core.Data.Validation;

namespace SimpleForum.Core.Data.ViewModels;

public class CreateBlogViewModel : BlogViewModel
{
    [Display(Name = "Cover Image")]
    [Required]
    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile CoverImage { get; set; } = null!;
}
