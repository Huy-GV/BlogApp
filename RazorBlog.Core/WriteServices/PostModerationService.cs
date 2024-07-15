using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using RazorBlog.Core.Models;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Constants;
using Microsoft.FeatureManagement;
using RazorBlog.Core.Features;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Core.WriteServices;

internal class PostModerationService : IPostModerationService
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly ILogger<UserModerationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserModerationService _userModerationService;
    private readonly IPostDeletionScheduler _postDeletionScheduler;
    private readonly IUserPermissionValidator _userPermissionValidator;
    private readonly IFeatureManager _featureManager;

    public PostModerationService(
        RazorBlogDbContext dbContext,
        ILogger<UserModerationService> logger,
        UserManager<ApplicationUser> userManager,
        IUserModerationService userModerationService,
        IPostDeletionScheduler postDeletionScheduler,
        IUserPermissionValidator userPermissionValidator,
        IFeatureManager featureManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _userModerationService = userModerationService;
        _postDeletionScheduler = postDeletionScheduler;
        _userPermissionValidator = userPermissionValidator;
        _featureManager = featureManager;
    }

    private async Task<bool> IsUserInAdminRole(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        return user != null && await _userManager.IsInRoleAsync(user, Roles.AdminRole);
    }

    private static void CensorDeletedComment(Comment comment)
    {
        comment.IsHidden = false;
        comment.Body = ReplacementText.RemovedContent;
        comment.ToBeDeleted = true;
    }

    private static void CensorDeletedBlog(Blog blog)
    {
        blog.IsHidden = false;
        blog.ToBeDeleted = true;
        blog.Title = ReplacementText.RemovedContent;
        blog.Introduction = ReplacementText.RemovedContent;
        blog.Body = ReplacementText.RemovedContent;
    }

    public async Task<ServiceResultCode> HideCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError("Comment with ID {commentId} not found", commentId);
            return ServiceResultCode.NotFound;
        }

        if (!await _userPermissionValidator.IsUserAllowedToHidePostAsync(userName, comment.AuthorUserName))
        {
            _logger.LogError("Comment with ID {commentId} cannot be hidden by user {userName}", commentId, userName);
            return ServiceResultCode.Unauthorized;
        }

        comment.IsHidden = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> HideBlogAsync(int blogId, string userName)
    {
        var blog = await _dbContext.Blog
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            _logger.LogError("Blog with ID {blogId} not found", blogId);
            return ServiceResultCode.NotFound;
        }

        if (!await _userPermissionValidator.IsUserAllowedToHidePostAsync(userName, blog.AuthorUserName))
        {
            _logger.LogError(message: "Blog with ID {blogId} cannot be hidden by user {userName}", blogId, userName);
            return ServiceResultCode.Unauthorized;
        }

        blog.IsHidden = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UnhideCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError("Comment with ID {commentId} not found", commentId);
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError("Comment with ID {commentId} cannot be un-hidden by user {userName}", commentId, userName);
            return ServiceResultCode.Unauthorized;
        }

        comment.IsHidden = false;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UnhideBlogAsync(int blogId, string userName)
    {
        var blog = await _dbContext.Blog
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            _logger.LogError("Blog with ID {blogId} not found", blogId);
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError(message: "Blog with ID {blogId} cannot be un-hidden by user {userName}", blogId, userName);
            return ServiceResultCode.Unauthorized;
        }

        blog.IsHidden = false;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ForciblyDeleteCommentAsync(int commentId, string deletorUserName)
    {
        if (!await IsUserInAdminRole(deletorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (!comment.IsHidden)
        {
            _logger.LogError("Comment with ID {commentId} must be hidden before being forcibly deleted", commentId);
            return ServiceResultCode.Unauthorized;
        }


        if (await _featureManager.IsEnabledAsync(FeatureNames.UseHangFire))
        {
            CensorDeletedComment(comment);
            _postDeletionScheduler.ScheduleCommentDeletion(
                new DateTimeOffset(DateTime.UtcNow.AddDays(7)),
                commentId);
        }
        else
        {
            _dbContext.Comment.Remove(comment);
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ForciblyDeleteBlogAsync(int blogId, string deletorUserName)
    {
        if (!await IsUserInAdminRole(deletorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var blog = await _dbContext.Blog
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (!blog.IsHidden)
        {
            _logger.LogError("Blog with ID {blogId} must be hidden before being forcibly deleted", blogId);
            return ServiceResultCode.Unauthorized;
        }

        if (await _featureManager.IsEnabledAsync(FeatureNames.UseHangFire))
        {
            CensorDeletedBlog(blog);
            _postDeletionScheduler.ScheduleBlogDeletion(
                new DateTimeOffset(DateTime.UtcNow.AddDays(7)),
                blogId);
        }
        else
        {
            _dbContext.Blog.Remove(blog);
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }
}
