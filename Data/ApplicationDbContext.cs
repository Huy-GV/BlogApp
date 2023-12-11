using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Models;

namespace RazorBlog.Data;

public class RazorBlogDbContext(DbContextOptions<RazorBlogDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Blog> Blog { get; set; } = null!;
    public DbSet<Comment> Comment { get; set; } = null!;
    public DbSet<BanTicket> BanTicket { get; set; } = null!;
    public DbSet<ApplicationUser> ApplicationUser { get; set; } = null!;

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