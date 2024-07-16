using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SimpleForum.Core.Data.Validation;

namespace SimpleForum.Core.Data.ViewModels;

public class EditUserViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Description { get; set; } = string.Empty;

    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile? NewProfilePicture { get; set; }
}
