using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.ViewModels;
using RazorBlog.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RazorBlog.Core.Services;

internal class BlogContentManager : IBlogContentManager
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserModerationService _userModerationService;
    private readonly IImageStore _imageStore;
    private readonly ILogger<BlogContentManager> _logger;
    private readonly IUserPermissionValidator _userPermissionValidator;

    public BlogContentManager(RazorBlogDbContext dbContext,
        IUserModerationService userModerationService,
        UserManager<ApplicationUser> userManager,
        IImageStore imageStore,
        ILogger<BlogContentManager> logger,
        IUserPermissionValidator userPermissionValidator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _userModerationService = userModerationService;
        _imageStore = imageStore;
        _logger = logger;
        _userPermissionValidator = userPermissionValidator;
    }

    public async Task<ServiceResultCode> DeleteBlogAsync(int blogId, string userName)
    {
        var blog = await _dbContext.Blog
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            return ServiceResultCode.NotFound;
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        var isCurrentUserAuthor = !string.IsNullOrWhiteSpace(user.UserName) &&
            user.UserName == blog.AuthorUser.UserName;

        if (!isCurrentUserAuthor ||
            blog.IsHidden ||
            await _userModerationService.BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Blog.Remove(blog);
        await _dbContext.SaveChangesAsync();

        return ServiceResultCode.Success;
    }

    public async Task<ServiceResultCode> UpdateBlog(EditBlogViewModel editBlogViewModel, string userName)
    {
        if (!Validator.TryValidateObject(editBlogViewModel, new ValidationContext(editBlogViewModel), null))
        {
            return ServiceResultCode.InvalidArguments;
        }

        var blog = await _dbContext.Blog
            .Include(x => x.AuthorUser)
            .FirstOrDefaultAsync(x => x.Id == editBlogViewModel.Id);

        if (blog == null)
        {
            return ServiceResultCode.NotFound;
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ServiceResultCode.Unauthorized;
        }

        if (!await _userPermissionValidator.IsUserAllowedToUpdateOrDeletePostAsync(userName, blog.IsHidden, blog.AuthorUserName))
        {
            return ServiceResultCode.Unauthorized;
        }

        _dbContext.Blog.Update(blog);
        blog.LastUpdateTime = DateTime.UtcNow;
        blog.Title = editBlogViewModel.Title;
        blog.Introduction = editBlogViewModel.Introduction;
        blog.Body = editBlogViewModel.Body;

        if (editBlogViewModel.CoverImage != null)
        {
            _logger.LogInformation("Replacing cover image of blog with ID {id}", editBlogViewModel.Id);

            await _imageStore.DeleteImage(blog.CoverImageUri);
            var (result, imageUri) = await _imageStore.UploadBlogCoverImageAsync(editBlogViewModel.CoverImage);
            if (result != ServiceResultCode.Success)
            {
                _logger.LogError("Failed to upload new blog cover image");
                return result;
            }

            blog.CoverImageUri = imageUri!;
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }

    public async Task<(ServiceResultCode, int?)> CreateBlogAsync(CreateBlogViewModel createBlogViewModel, string userName)
    {
        if (!Validator.TryValidateObject(createBlogViewModel, new ValidationContext(createBlogViewModel), null))
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

        var (result, imageUri) = await _imageStore.UploadBlogCoverImageAsync(createBlogViewModel.CoverImage);
        if (result != ServiceResultCode.Success)
        {
            return (result, null);
        }

        var now = DateTime.UtcNow;
        var newBlog = new Blog
        {
            Title = createBlogViewModel.Title,
            Introduction = createBlogViewModel.Introduction,
            Body = createBlogViewModel.Body,
            CreationTime = now,
            LastUpdateTime = now,
            AuthorUserName = userName,
        };

        _dbContext.Blog.Add(newBlog);
        newBlog.CoverImageUri = imageUri!;
        await _dbContext.SaveChangesAsync();

        return (ServiceResultCode.Success, newBlog.Id);
    }
}
