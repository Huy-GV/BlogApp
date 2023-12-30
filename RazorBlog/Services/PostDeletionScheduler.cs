using System;
using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;

namespace RazorBlog.Services;

public class PostDeletionScheduler(
    RazorBlogDbContext dbContext,
    ILogger<IPostDeletionScheduler> logger) : IPostDeletionScheduler
{
    private readonly RazorBlogDbContext _dbContext = dbContext;
    private readonly ILogger<IPostDeletionScheduler> _logger = logger;

    public void ScheduleBlogDeletion(DateTimeOffset deleteTime, int blogId)
    {
        _logger.LogInformation($"Blog ID {blogId} scheduled for deletion");
        Expression<Action> deleteBlog = () => DeleteBlog(blogId);
        BackgroundJob.Schedule(deleteBlog, deleteTime);
    }

    public void DeleteBlog(int blogId)
    {
        var blogToDelete = _dbContext.Blog.Find(blogId);
        if (blogToDelete == null)
        {
            _logger.LogInformation($"Blog ID {blogId} already deleted ahead of schedule");
            return;
        }

        _dbContext.Blog.Remove(blogToDelete);
        _dbContext.SaveChanges();
    }

    public void DeleteComment(int commentId)
    {
        _logger.LogInformation($"Comment ID {commentId} scheduled for deletion");
        var commentToDelete = _dbContext.Comment.Find(commentId);
        if (commentToDelete == null)
        {
            _logger.LogInformation($"Comment ID {commentId} already deleted ahead of schedule");
            return;
        }

        _dbContext.Comment.Remove(commentToDelete);
        _dbContext.SaveChanges();
    }

    public void ScheduleCommentDeletion(DateTimeOffset deleteTime, int commentId)
    {
        Expression<Action> deleteComment = () => DeleteComment(commentId);
        BackgroundJob.Schedule(deleteComment, deleteTime);
    }
}
