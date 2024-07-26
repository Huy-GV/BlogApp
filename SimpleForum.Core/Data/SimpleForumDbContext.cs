using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Models;

namespace SimpleForum.Core.Data;

public class SimpleForumDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
{
    public SimpleForumDbContext(DbContextOptions<SimpleForumDbContext> options) : base(options)
    {
    }

    public DbSet<Thread> Thread { get; set; } = null!;
    public DbSet<Comment> Comment { get; set; } = null!;
    public DbSet<BanTicket> BanTicket { get; set; } = null!;
    public DbSet<ApplicationUser> ApplicationUser { get; set; } = null!;
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Thread>(entity =>
        {
            entity
                .HasQueryFilter(x => !x.ToBeDeleted)
                .HasMany(x => x.Comments)
                .WithOne()
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.AuthorUser)
                .WithMany()
                .HasPrincipalKey(x => x.UserName)
                .HasForeignKey(x => x.AuthorUserName)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Comment>()
            .HasOne(x => x.AuthorUser)
            .WithMany()
            .HasPrincipalKey(x => x.UserName)
            .HasForeignKey(x => x.AuthorUserName)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<BanTicket>()
            .HasOne(x => x.AppUser)
            .WithOne()
            .HasPrincipalKey<ApplicationUser>(x => x.UserName)
            .HasForeignKey<BanTicket>(x => x.UserName)
            .IsRequired();
    }
}
