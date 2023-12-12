using System;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Models;

namespace RazorBlog.Services;

public class UserModerationService(
    RazorBlogDbContext dbContext,
    ILogger<UserModerationService> logger,
    UserManager<ApplicationUser> userManager) : IUserModerationService
{
    private readonly RazorBlogDbContext _dbContext = dbContext;
    private readonly ILogger<UserModerationService> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<bool> BanTicketExistsAsync(string username)
    {
        return await _dbContext.BanTicket.AnyAsync(s => s.UserName == username);
    }

    public async Task<BanTicket?> FindAsync(string username)
    {
        return await _dbContext
            .BanTicket
            .FirstOrDefaultAsync(s => s.UserName == username);
    }

    public async Task HideCommentAsync(int commentId)
    {
        var comment = await _dbContext.Comment.FindAsync(commentId);
        if (comment == null)
        {
            _logger.LogError($"Comment with ID {commentId} not found");
            return;
        }

        comment.IsHidden = true;
        _dbContext.Comment.Update(comment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task HideBlogAsync(int blogId)
    {
        var blog = await _dbContext.Blog.FindAsync(blogId);
        if (blog == null)
        {
            _logger.LogError($"Blog with ID {blogId} not found");
            return;
        }

        blog.IsHidden = true;
        _dbContext.Blog.Update(blog);
        await _dbContext.SaveChangesAsync();
    }

    public async Task BanUser(string userName, DateTime? expiry)
    {
        if (await BanTicketExistsAsync(userName))
        {
            _logger.LogInformation($"User {userName} has already been banned");
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            _logger.LogError($"User {userName} not found");
            return;
        }

        await _userManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);
        _dbContext.BanTicket.Add(new BanTicket() { UserName = userName, Expiry = expiry });
        await _dbContext.SaveChangesAsync();

        if (!expiry.HasValue)
        {
            return;
        }

        BackgroundJob.Schedule(() => RemoveBanTicketAsync(userName), new DateTimeOffset(expiry.Value));
    }

    public async Task RemoveBanTicketAsync(string userName)
    {
        var banTicket = await FindAsync(userName);
        if (banTicket == null)
        {
            _logger.LogWarning($"Ban ticket for user {userName} already removed");
            return;
        }

        try
        {
            _dbContext.BanTicket.Remove(banTicket);
            await _dbContext.SaveChangesAsync();
        } 
        catch (DBConcurrencyException)
        {
            _logger.LogWarning($"Ban ticket for user {banTicket.UserName} already removed");
        }
    }
}