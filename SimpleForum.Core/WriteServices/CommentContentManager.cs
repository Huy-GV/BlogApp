using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Models;
using SimpleForum.Core.ReadServices;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SimpleForum.Core.WriteServices;

public class CommentContentManager : ICommentContentManager
{
    private readonly SimpleForumDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBanTicketReader _banTicketReader;
    private readonly IUserPermissionValidator _userPermissionValidator;

    public CommentContentManager(SimpleForumDbContext dbContext,
        IBanTicketReader banTicketReader,
        IUserPermissionValidator userPermissionValidator,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _banTicketReader = banTicketReader;
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
            await _banTicketReader.BanTicketExistsAsync(userName))
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
                comment.ReportTicketId != null,
                comment.AuthorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

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
            ThreadId = createCommentViewModel.ThreadId,
            Body = createCommentViewModel.Body,
            CreationTime = creationTime,
            LastUpdateTime = creationTime,
        });

        await _dbContext.SaveChangesAsync();

        return (ServiceResultCode.Success, createCommentViewModel.ThreadId);
    }
}
