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

    public DbSet<Thread> Thread => Set<Thread>();
    public DbSet<Comment> Comment => Set<Comment>();
    public DbSet<BanTicket> BanTicket => Set<BanTicket>();
    public DbSet<ReportTicket> ReportTicket => Set<ReportTicket>();
    public DbSet<ApplicationUser> ApplicationUser => Set<ApplicationUser>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Thread>(entity =>
        {
            entity
                .HasQueryFilter(x => !x.ToBeDeleted)
                .HasMany(x => x.Comments)
                .WithOne()
                .HasForeignKey(x => x.ThreadId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.AuthorUser)
                .WithMany()
                .HasPrincipalKey(x => x.UserName)
                .HasForeignKey(x => x.AuthorUserName)
                .OnDelete(DeleteBehavior.NoAction);

            // report ticket might be for a comment on the thread, not the thread itself
            entity
                .HasOne(x => x.ReportTicket)
                .WithMany()
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.ReportTicketId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Comment>(entity =>
        {
            entity
                .HasOne(x => x.AuthorUser)
                .WithMany()
                .HasPrincipalKey(x => x.UserName)
                .HasForeignKey(x => x.AuthorUserName)
                .OnDelete(DeleteBehavior.NoAction);

            entity
                .HasOne(x => x.ReportTicket)
                .WithOne()
                .HasForeignKey<Comment>(x => x.ReportTicketId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ReportTicket>(entity =>
        {
            entity
                .HasOne(x => x.ReportingUser)
                .WithMany()
                .HasPrincipalKey(x => x.UserName)
                .HasForeignKey(x => x.ReportingUserName)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            // report ticket might be for a comment on the thread, not the thread itself
            entity
                .HasOne<Thread>()
                .WithMany()
                .HasForeignKey(x => x.ThreadId)
                .HasPrincipalKey(x => x.Id)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);

            entity
                .HasOne<Comment>()
                .WithOne()
                .HasForeignKey<ReportTicket>(x => x.CommentId)
                .HasPrincipalKey<Comment>(x => x.Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<BanTicket>()
            .HasOne(x => x.AppUser)
            .WithOne()
            .HasPrincipalKey<ApplicationUser>(x => x.UserName)
            .HasForeignKey<BanTicket>(x => x.UserName)
            .IsRequired();
    }
}
