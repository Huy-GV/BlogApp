using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using BlogApp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;

namespace BlogApp.Services
{
    // todo: remove ban with a background service
    public class UserModerationService : IUserModerationService
    {
        private readonly ILogger<UserModerationService> _logger;
        private readonly RazorBlogDbContext _dbContext;

        public UserModerationService(
            RazorBlogDbContext dbContext,
            ILogger<UserModerationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> BanTicketExistsAsync(string username)
        {
            await CheckExpiryAsync(username);

            return _dbContext.BanTicket.Any(s => s.UserName == username);
        }

        [Obsolete("To be replaced by background service.")]
        private async Task CheckExpiryAsync(string username)
        {
            var suspension = await FindAsync(username);
            if (suspension != null)
            {
                await RemoveAsync(suspension);
            }
        }

        public async Task<BanTicket> FindAsync(string username)
        {
            return await _dbContext
                .BanTicket
                .SingleOrDefaultAsync(s => s.UserName == username);
        }

        public async Task HideCommentAsync(int commentId)
        {
            var comment = await _dbContext.Comment.FindAsync(commentId);
            if (comment == null)
            {
                _logger.LogError($"Comment with ID {commentId} not found");
                return;
            }

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

            _dbContext.Blog.Update(blog);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveAsync(BanTicket ticket)
        {
            _dbContext.BanTicket.Remove(ticket);
            await _dbContext.SaveChangesAsync();
        }
    }
}