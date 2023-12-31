﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
using RazorBlog.Data.Constants;
using RazorBlog.Data;
using RazorBlog.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace RazorBlog.Services;

public class PostModerationService(
    RazorBlogDbContext dbContext,
    ILogger<UserModerationService> logger,
    UserManager<ApplicationUser> userManager,
    IUserModerationService userModerationService,
    IPostDeletionScheduler postDeletionScheduler) : IPostModerationService
{
    private readonly RazorBlogDbContext _dbContext = dbContext;
    private readonly ILogger<UserModerationService> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserModerationService _userModerationService = userModerationService;
    private readonly IPostDeletionScheduler _postDeletionScheduler = postDeletionScheduler;

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

        if (await _userModerationService.BanTicketExistsAsync(user.UserName ?? string.Empty))
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

    private async Task<bool> IsUserInAdminRole(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        return user != null && await _userManager.IsInRoleAsync(user, Roles.AdminRole);
    }

    private static void CensorDeletedComment(Comment comment)
    {
        comment.IsHidden = false;
        comment.Content = ReplacementText.RemovedContent;
        comment.ToBeDeleted = true;
    }

    private static void CensorDeletedBlog(Blog blog)
    {
        blog.IsHidden = false;
        blog.ToBeDeleted = true;
        blog.Title = ReplacementText.RemovedContent;
        blog.Introduction = ReplacementText.RemovedContent;
        blog.Content = ReplacementText.RemovedContent;
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

    public async Task<ServiceResultCode> UnhideCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            _logger.LogError($"Comment with ID {commentId} not found");
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError($"Comment with ID {commentId} cannot be un-hidden by user {userName}");
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Comment.Update(comment);
        comment.IsHidden = false;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UnhideBlogAsync(int blogId, string userName)
    {
        var blog = await _dbContext.Blog
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            _logger.LogError($"Blog with ID {blogId} not found");
            return ServiceResultCode.NotFound;
        }

        if (!await IsUserInAdminRole(userName))
        {
            _logger.LogError(message: $"Blog with ID {blogId} cannot be un-hidden by user {userName}");
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Blog.Update(blog);
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
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (!comment.IsHidden)
        {
            _logger.LogError($"Comment ID {commentId} must be hidden before being forcibly deleted");
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Comment.Update(comment);
        CensorDeletedComment(comment);
        await _dbContext.SaveChangesAsync();

        _postDeletionScheduler.ScheduleCommentDeletion(
            new DateTimeOffset(DateTime.UtcNow.AddDays(7)), 
            commentId);

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> ForciblyDeleteBlogAsync(int blogId, string deletorUserName)
    {
        if (!await IsUserInAdminRole(deletorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        var blog = await _dbContext.Blog
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            return ServiceResultCode.NotFound;
        }

        if (!blog.IsHidden)
        {
            _logger.LogError($"Blog ID {blogId} must be hidden before being forcibly deleted");
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Blog.Update(blog);
        CensorDeletedBlog(blog);
        await _dbContext.SaveChangesAsync();

        _postDeletionScheduler.ScheduleBlogDeletion(
            new DateTimeOffset(DateTime.UtcNow.AddDays(7)),
            blogId);

        return ServiceResultCode.Success;
    }
}