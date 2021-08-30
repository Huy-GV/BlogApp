using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
public class BlogUser : IdentityUser
{
    [DataType(DataType.Date)]
    public DateTime? CakeDay { get; set; }
    public int BanCount { get; set; } = 0;
}