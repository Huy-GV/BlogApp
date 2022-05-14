namespace RazorBlog.Data.DTOs;

public class BaseProfileDto
{
    public string UserName { get; set; }
    public string ProfilePicturePath { get; set; } = "default";
}