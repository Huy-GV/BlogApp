using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class EditUserViewModel
{
    [Required]
    public string UserName { get; set; }
    public string Description { get; set; }
    public IFormFile? NewProfilePicture { get; set; }
}