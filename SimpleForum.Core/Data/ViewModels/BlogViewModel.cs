using System.ComponentModel.DataAnnotations;

namespace SimpleForum.Core.Data.ViewModels;

public class BlogViewModel
{
    [Required]
    [StringLength(60, MinimumLength = 10)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 200)]
    public string Body { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string Introduction { get; set; } = string.Empty;
}
