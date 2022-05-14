using System.Collections.Generic;
using RazorBlog.Models;

namespace RazorBlog.Data.DTOs;

public class PersonalProfileDto : BaseProfileDto
{
    public string Country { get; set; }
    public string Description { get; set; } = "None";
    public string RegistrationDate { get; set; }
    public bool IsModerator { get; set; } = false;
    public uint BlogCount { get; set; }
    public uint BlogCountCurrentYear { get; set; }
    public uint CommentCount { get; set; }
    public uint ViewCountCurrentYear { get; set; } = 0;
    public List<Blog> Blogs { get; set; } = new();
}