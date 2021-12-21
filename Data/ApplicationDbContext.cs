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
        public DbSet<Blog> Blog { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Suspension> Suspension { get; set;}
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
