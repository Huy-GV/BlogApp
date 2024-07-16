using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SimpleForum.Core.Data.Validation;

namespace SimpleForum.Core.Data.ViewModels;

public class EditProfilePictureViewModel
{
    [Required]
    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile NewProfilePicture { get; set; } = null!;
}
