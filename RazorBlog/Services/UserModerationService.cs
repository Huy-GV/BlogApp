using System;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
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

    private async Task<bool> IsUserAllowedToHidePostAsync(string userName, Post post)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return false;
        }

        if (user.UserName == post.AppUser.UserName)
        {
            return false;
        }

        if (await BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.ModeratorRole) && 
            !await _userManager.IsInRoleAsync(user, Roles.AdminRole))
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(post.AppUser, Roles.AdminRole))
        {
            _logger.LogError($"Posts authored by admin users cannot be hidden");
            return false;
        }

        return true;
    }

    public async Task<bool> BanTicketExistsAsync(string userName)
    {
        return await _dbContext.BanTicket.AnyAsync(s => s.UserName == userName);
    }

    public async Task<BanTicket?> FindByUserNameAsync(string userName)
    {
        return await _dbContext
            .BanTicket
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(s => s.UserName == userName);
    }

    public async Task<ServiceResultCode> HideCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError($"Comment with ID {commentId} not found");
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserAllowedToHidePostAsync(userName, comment))
        {
            _logger.LogError($"Comment with ID {commentId} cannot be hidden by user {userName}");
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Comment.Update(comment);
        comment.IsHidden = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> HideBlogAsync(int blogId, string userName)
    {
        var blog = await _dbContext.Blog
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            _logger.LogError($"Blog with ID {blogId} not found");
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserAllowedToHidePostAsync(userName, blog))
        {
            _logger.LogError(message: $"Blog with ID {blogId} cannot be hidden by user {userName}");
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Blog.Update(blog);
        blog.IsHidden = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task BanUserAsync(string userToBanName, DateTime? expiry)
    {
        if (await BanTicketExistsAsync(userToBanName))
        {
            _logger.LogInformation($"User {userToBanName} has already been banned");
        }

        var user = await _userManager.FindByNameAsync(userToBanName);
        if (user == null)
        {
            _logger.LogError($"User {userToBanName} not found");
            return;
        }

        await _userManager.RemoveFromRoleAsync(user, Roles.ModeratorRole);
        _dbContext.BanTicket.Add(new BanTicket() { UserName = userToBanName, Expiry = expiry });
        await _dbContext.SaveChangesAsync();

        if (!expiry.HasValue)
        {
            return;
        }

        BackgroundJob.Schedule(() => RemoveBanTicketAsync(userToBanName), new DateTimeOffset(expiry.Value));
    }

    public async Task RemoveBanTicketAsync(string bannedUserName)
    {
        var banTicket = await FindByUserNameAsync(bannedUserName);
        if (banTicket == null)
        {
            _logger.LogWarning($"Ban ticket for user {bannedUserName} already removed");
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