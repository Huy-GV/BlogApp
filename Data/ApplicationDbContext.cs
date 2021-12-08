using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BlogApp.Models;

namespace BlogApp.Data
{
    public class RazorBlogDbContext : IdentityDbContext<ApplicationUser>
    {
        public RazorBlogDbContext(DbContextOptions<RazorBlogDbContext> options)
            : base(options)
        {
        }
        public DbSet<BlogApp.Models.Blog> Blog { get; set; }
        public DbSet<BlogApp.Models.Comment> Comment { get; set; }
        public DbSet<BlogApp.Models.Suspension> Suspension { get; set;}
        public DbSet<BlogApp.Models.ApplicationUser> ApplicationUser { get; set; }
    }
}
