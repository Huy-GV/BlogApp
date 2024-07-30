using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SimpleForum.Core.Data.Validation;

namespace SimpleForum.Core.Data.ViewModels;

public class CreateUserViewModel
{
    [Required]
    [Display(Name = "UserName")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Profile Picture (optional)")]
    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile? ProfilePicture { get; set; }
}
