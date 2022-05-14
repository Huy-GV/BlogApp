using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class EditUserViewModel
{
    public string UserName { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Country { get; set; }

    [Required]
    [StringLength(350, MinimumLength = 20)]
    public string Description { get; set; }

    public IFormFile? NewProfilePicture { get; set; }
}