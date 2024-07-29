using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Models;
using SimpleForum.Core.QueryServices;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SimpleForum.Core.CommandServices;

internal class ThreadContentManager : IThreadContentManager
{
    private readonly SimpleForumDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBanTicketReader _banTicketReader;
    private readonly IImageStore _imageStore;
    private readonly ILogger<ThreadContentManager> _logger;
    private readonly IUserPermissionValidator _userPermissionValidator;

    public ThreadContentManager(SimpleForumDbContext dbContext,
        IBanTicketReader banTicketReader,
        UserManager<ApplicationUser> userManager,
        IImageStore imageStore,
        ILogger<ThreadContentManager> logger,
        IUserPermissionValidator userPermissionValidator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _banTicketReader = banTicketReader;
        _imageStore = imageStore;
        _logger = logger;
        _userPermissionValidator = userPermissionValidator;
    }

    public async Task<ServiceResultCode> DeleteThreadAsync(int threadId, string userName)
    {
        var thread = await _dbContext.Thread
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == threadId);

        if (thread == null)
        {
            return ServiceResultCode.NotFound;
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        var isCurrentUserAuthor = !string.IsNullOrWhiteSpace(user.UserName) &&
            user.UserName == thread.AuthorUser.UserName;

        if (!isCurrentUserAuthor ||
            thread.ReportTicketId != null ||
            await _banTicketReader.BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Thread.Remove(thread);
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UpdateThreadAsync(EditThreadViewModel editThreadViewModel, string userName)
    {
        if (!Validator.TryValidateObject(editThreadViewModel, new ValidationContext(editThreadViewModel), null))
        {
            return ServiceResultCode.InvalidArguments;
        }

        var thread = await _dbContext.Thread
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == editThreadViewModel.Id);

        if (thread == null)
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
            thread.ReportTicketId != null,
            thread.AuthorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        thread.LastUpdateTime = DateTime.UtcNow;
        thread.Title = editThreadViewModel.Title;
        thread.Introduction = editThreadViewModel.Introduction;
        thread.Body = editThreadViewModel.Body;

        if (editThreadViewModel.CoverImage != null)
        {
            _logger.LogInformation("Replacing cover image of thread with ID {id}", editThreadViewModel.Id);

            await _imageStore.DeleteImage(thread.CoverImageUri);
            var (result, imageUri) = await _imageStore.UploadThreadCoverImageAsync(editThreadViewModel.CoverImage);
            if (result != ServiceResultCode.Success)
            {
                _logger.LogError("Failed to upload new thread cover image");
                return result;
            }

            thread.CoverImageUri = imageUri!;
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }

    public async Task<(ServiceResultCode, int?)> CreateThreadAsync(CreateThreadViewModel createThreadViewModel, string userName)
    {
        if (!Validator.TryValidateObject(createThreadViewModel, new ValidationContext(createThreadViewModel), null))
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

        var (result, imageUri) = await _imageStore.UploadThreadCoverImageAsync(createThreadViewModel.CoverImage);
        if (result != ServiceResultCode.Success)
        {
            return (result, null);
        }

        var now = DateTime.UtcNow;
        var newThread = new Thread
        {
            Title = createThreadViewModel.Title,
            Introduction = createThreadViewModel.Introduction,
            Body = createThreadViewModel.Body,
            CreationTime = now,
            LastUpdateTime = now,
            AuthorUserName = userName,
        };

        _dbContext.Thread.Add(newThread);
        newThread.CoverImageUri = imageUri!;
        await _dbContext.SaveChangesAsync();

        return (ServiceResultCode.Success, newThread.Id);
    }
}
