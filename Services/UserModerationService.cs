using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Models;

namespace RazorBlog.Services;

// todo: remove ban with a background service
public class UserModerationService(
    RazorBlogDbContext dbContext,
    ILogger<UserModerationService> logger) : IUserModerationService
{
    private readonly RazorBlogDbContext _dbContext = dbContext;
    private readonly ILogger<UserModerationService> _logger = logger;

    public async Task<bool> BanTicketExistsAsync(string username)
    {
        // await CheckExpiryAsync(username);

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

    public async Task RemoveBanTicketAsync(BanTicket ticket)
    {
        _dbContext.BanTicket.Remove(ticket);
        await _dbContext.SaveChangesAsync();
    }

    [Obsolete("To be replaced by background service.")]
    private async Task CheckExpiryAsync(string username)
    {
        var suspension = await FindAsync(username);
        if (suspension != null)
        {
            await RemoveBanTicketAsync(suspension);
        }
    }
}