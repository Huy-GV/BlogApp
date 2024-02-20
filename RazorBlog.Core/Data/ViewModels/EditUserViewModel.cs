using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RazorBlog.Core.Data.Validation;

namespace RazorBlog.Core.Data.ViewModels;

public class EditUserViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Description { get; set; } = string.Empty;

    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile? NewProfilePicture { get; set; }
}