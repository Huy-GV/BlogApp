using System;

namespace RazorBlog.Data.DTOs;

[Obsolete]
public class BaseProfileDto
{
    public string UserName { get; set; }
    public string ProfilePicturePath { get; set; } = "default";
}