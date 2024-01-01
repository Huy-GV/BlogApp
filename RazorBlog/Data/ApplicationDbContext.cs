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
            .WithOne()
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Blog>()
            .HasQueryFilter(blog => !blog.ToBeDeleted);

        builder.Entity<ApplicationUser>()
            .HasMany(x => x.Blogs)
            .WithOne(x => x.AuthorUser)
            .HasForeignKey(x => x.AuthorUserName)
            .HasPrincipalKey(x => x.UserName)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Comment>()
            .HasOne(x => x.AuthorUser)
            .WithMany()
            .HasPrincipalKey(x => x.UserName)
            .HasForeignKey(x => x.AuthorUserName)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<BanTicket>()
            .HasOne(b => b.AppUser)
            .WithOne()
            .HasPrincipalKey<ApplicationUser>(x => x.UserName)
            .HasForeignKey<BanTicket>(x => x.UserName)
            .IsRequired();
    }
}