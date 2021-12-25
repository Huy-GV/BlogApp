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
    public class UserModerationService
    {
        private readonly string _inappropriateBlog = "The blog has been hidden due to inappropriate content. The admin will decide whether it gets removed or not";
        private readonly string _inappropriateComment = "The comment has been hidden due to inappropriate content. The admin will decide whether it gets removed or not";
        private readonly ILogger<ImageService> _logger; 
        private readonly RazorBlogDbContext _dbContext;
        public UserModerationService(
            RazorBlogDbContext dbContext, 
            ILogger<ImageService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<bool> ExistsAsync(string username)
        {
            await CheckExpiryAsync(username);
            return _dbContext.Suspension.Any(s => s.Username == username);
        }
        private async Task CheckExpiryAsync(string username)
        {
            var suspension = await FindAsync(username);
            if (suspension != null && DateTime.Compare(DateTime.Now, suspension.Expiry) > 0)
            {
                await RemoveAsync(suspension);
            }
        }

        public async Task<Suspension> FindAsync(string username) 
        {
            return await _dbContext
            .Suspension
            .FirstOrDefaultAsync(s => s.Username == username);
        } 
        public async Task HideCommentAsync(int commentID) 
        {
            var comment = await _dbContext.Comment.FindAsync(commentID);
            if (comment == null)
            {
                _logger.LogError($"Blog with ID {commentID} not found");
                return;
            }
            comment.SuspensionExplanation = _inappropriateComment;
            _dbContext.Comment.Update(comment);
            await _dbContext.SaveChangesAsync();
        }
        public async Task HideBlogAsync(int blogID) 
        {
            var blog = await _dbContext.Blog.FindAsync(blogID);
            if (blog == null)
            {
                _logger.LogError($"Blog with ID {blogID} not found");
                return;
            }

            blog.SuspensionExplanation = _inappropriateBlog;
            _dbContext.Blog.Update(blog);
            await _dbContext.SaveChangesAsync();
        }
        public async Task RemoveAsync(Suspension suspension)
        {
            //TODO: missing .Suspension
            _dbContext.Remove(suspension);
            await _dbContext.SaveChangesAsync();
        }
    }
}
