using System;
using System.Linq;
using Hangfire;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;

namespace RazorBlog.Core.WriteServices;

internal class PostDeletionScheduler : IPostDeletionScheduler
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly ILogger<IPostDeletionScheduler> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public PostDeletionScheduler(
        RazorBlogDbContext dbContext,
        ILogger<IPostDeletionScheduler> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        _dbContext = dbContext;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId)
    {
        _logger.LogInformation("Blog with ID {blogId} scheduled for deletion", blogId);
        _backgroundJobClient.Schedule(() => DeleteBlog(blogId), deleteTime);
    }

    public void DeleteBlog(int blogId)
    {
        var blogToDelete = _dbContext.Blog.FirstOrDefault(x => x.Id == blogId);
        if (blogToDelete == null)
        {
            _logger.LogInformation("Blog with ID {blogId} already deleted ahead of schedule", blogId);
            return;
        }

        _dbContext.Blog.Remove(blogToDelete);
        _dbContext.SaveChanges();
    }

    public void DeleteComment(int commentId)
    {
        _logger.LogInformation("Comment with ID {commentId} scheduled for deletion", commentId);
        var commentToDelete = _dbContext.Comment.FirstOrDefault(x => x.Id == commentId);
        if (commentToDelete == null)
        {
            _logger.LogInformation("Comment with ID {commentId} already deleted ahead of schedule", commentId);
            return;
        }

        _dbContext.Comment.Remove(commentToDelete);
        _dbContext.SaveChanges();
    }

    public void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId)
    {
        _backgroundJobClient.Schedule(() => DeleteComment(commentId), deleteTime);
    }
}
