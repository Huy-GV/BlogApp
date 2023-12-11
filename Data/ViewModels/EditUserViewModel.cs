using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class EditUserViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IFormFile? NewProfilePicture { get; set; }
}