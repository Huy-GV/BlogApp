using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Models;

namespace RazorBlog.Data;

public class RazorBlogDbContext : IdentityDbContext<ApplicationUser>
{
    public RazorBlogDbContext(DbContextOptions<RazorBlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blog { get; set; }
    public DbSet<Comment> Comment { get; set; }
    public DbSet<BanTicket> BanTicket { get; set; }
    public DbSet<ApplicationUser> ApplicationUser { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Blog>()
            .HasMany(b => b.Comments)
            .WithOne();

        builder.Entity<Blog>()
            .HasQueryFilter(blog => !blog.ToBeDeleted);

        builder.Entity<BanTicket>()
            .HasOne(b => b.AppUser)
            .WithMany()
            .HasForeignKey(b => b.UserName)
            .HasPrincipalKey(u => u.UserName)
            .IsRequired();

        builder.Entity<BanTicket>()
            .HasIndex(b => b.UserName)
            .IsUnique();
    }
}