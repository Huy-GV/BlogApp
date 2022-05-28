using System;
using System.Collections.Generic;
using RazorBlog.Models;

namespace RazorBlog.Data.DTOs;

public class PersonalProfileDto : BaseProfileDto
{
    public DateTime? RegistrationDate { get; set; }
    public string Description { get; set; } = "None";
    public bool IsModerator { get; set; } = false;
    public uint BlogCount { get; set; }
    public uint BlogCountCurrentYear { get; set; }
    public uint CommentCount { get; set; }
    public uint ViewCountCurrentYear { get; set; } = 0;
    // todo: only show blog title and date, with date being on the left outside the card
    public List<Blog> Blogs { get; set; } = new();
}