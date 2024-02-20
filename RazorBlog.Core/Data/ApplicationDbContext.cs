using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Core.Models;

namespace RazorBlog.Core.Data;

public class RazorBlogDbContext : IdentityDbContext<ApplicationUser>
{
    public RazorBlogDbContext(DbContextOptions<RazorBlogDbContext> options) : base(options)
    {
    }

    public DbSet<Blog> Blog { get; set; } = null!;
    public DbSet<Comment> Comment { get; set; } = null!;
    public DbSet<BanTicket> BanTicket { get; set; } = null!;
    public DbSet<ApplicationUser> ApplicationUser { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Blog>()
            .HasQueryFilter(blog => !blog.ToBeDeleted)
            .HasMany(b => b.Comments)
            .WithOne()
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Blog>()
            .HasOne(x => x.AuthorUser)
            .WithMany()
            .HasPrincipalKey(x => x.UserName)
            .HasForeignKey(x => x.AuthorUserName)
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