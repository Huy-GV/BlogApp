using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Communication;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RazorBlog.Services;

public class CommentContentManager : ICommentContentManager
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserModerationService _userModerationService;
    private readonly IUserPermissionValidator _userPermissionValidator;

    public CommentContentManager(RazorBlogDbContext dbContext,
        IUserModerationService userModerationService,
        IUserPermissionValidator userPermissionValidator,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _userModerationService = userModerationService;
        _userPermissionValidator = userPermissionValidator;
    }

    public async Task<ServiceResultCode> DeleteCommentAsync(int commentId, string userName)
    {
        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return ServiceResultCode.NotFound;
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        var isCurrentUserAuthor = !string.IsNullOrWhiteSpace(userName) &&
            user.UserName == comment.AuthorUser.UserName;

        if (!isCurrentUserAuthor ||
            comment.IsHidden ||
            await _userModerationService.BanTicketExistsAsync(userName))
        {
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Comment.Remove(comment);
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UpdateCommentAsync(
        int commentId,
        CommentViewModel editCommentViewModel,
        string userName)
    {
        if (!Validator.TryValidateObject(editCommentViewModel, new ValidationContext(editCommentViewModel), null))
        {
            return ServiceResultCode.InvalidArguments;
        }

        var comment = await _dbContext.Comment
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return ServiceResultCode.NotFound;
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        if (!await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(
                userName, 
                comment.IsHidden,
                comment.AuthorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Comment.Update(comment);

        comment.LastUpdateTime = DateTime.UtcNow;
        comment.Body = editCommentViewModel.Body;
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<(ServiceResultCode, int?)> CreateCommentAsync(
        CommentViewModel createCommentViewModel,
        string userName)
    {
        if (!Validator.TryValidateObject(createCommentViewModel, new ValidationContext(createCommentViewModel), null))
        {
            return (ServiceResultCode.InvalidArguments, null);
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return (ServiceResultCode.Unauthorized, null);
        }

        if (!await _userPermissionValidator.IsUserAllowedToCreatePostAsync(userName))
        {
            return (ServiceResultCode.Unauthorized, null);
        }

        var creationTime = DateTime.UtcNow;
        _dbContext.Comment.Add(new Comment
        {
            AuthorUserName = userName,
            BlogId = createCommentViewModel.BlogId,
            Body = createCommentViewModel.Body,
            CreationTime = creationTime,
            LastUpdateTime = creationTime,
        });

        await _dbContext.SaveChangesAsync();

        return (ServiceResultCode.Success, createCommentViewModel.BlogId);
    }
}
