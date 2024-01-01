using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RazorBlog.Data.ViewModels;

public class EditBlogViewModel : BlogViewModel
{
    [Required]
    public int Id { get; set; }

    [Display(Name = "Change cover image")]
    public IFormFile? CoverImage { get; set; }
}