using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SimpleForum.Core.Data.Validation;

namespace SimpleForum.Core.Data.ViewModels;

public class EditThreadViewModel : ThreadViewModel
{
    [Required]
    public int Id { get; set; }

    [Display(Name = "Change cover image")]
    [FileType("jpg", "jpeg", "png", ErrorMessage = "File type must be one of: .png, .jpeg, .jpg")]
    public IFormFile? CoverImage { get; set; }
}
