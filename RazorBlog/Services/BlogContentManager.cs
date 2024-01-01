using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Communication;
using RazorBlog.Data;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RazorBlog.Services;

public class BlogContentManager(
    RazorBlogDbContext dbContext,
    IUserModerationService userModerationService,
    IPostModerationService postModerationService,
    UserManager<ApplicationUser> userManager,
    IImageStorage imageStorage,
    ILogger<BlogContentManager> logger) : IBlogContentManager
{
    private readonly RazorBlogDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserModerationService _userModerationService = userModerationService;
    private readonly IPostModerationService _postModerationService = postModerationService;
    private readonly IImageStorage _imageStorage = imageStorage;
    private readonly ILogger<BlogContentManager> _logger = logger;

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

        if (!await _postModerationService.IsUserAllowedToUpdateOrDeletePostAsync(userName, blog))
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
            try
            {
                await _imageStorage.DeleteImage(blog.CoverImageUri);
                var imageName = await _imageStorage.UploadBlogCoverImageAsync(editBlogViewModel.CoverImage);
                blog.CoverImageUri = imageName;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update blog image");
                _logger.LogError(ex.Message);

                return ServiceResultCode.InvalidArguments;
            }
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResultCode.Success;
    }

    public async Task<(ServiceResultCode, int)> CreateBlogAsync(CreateBlogViewModel createBlogViewModel, string userName)
    {
        if (!Validator.TryValidateObject(createBlogViewModel, new ValidationContext(createBlogViewModel), null))
        {
            return (ServiceResultCode.InvalidArguments, 0);
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return (ServiceResultCode.Unauthorized, 0);
        }

        if (!await _postModerationService.IsUserAllowedToCreatePostAsync(userName))
        {
            return (ServiceResultCode.Unauthorized, 0);
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

        try
        {
            var imageUri = await _imageStorage.UploadBlogCoverImageAsync(createBlogViewModel.CoverImage);
            newBlog.CoverImageUri = imageUri;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create blog");
            _logger.LogError(ex.Message);

            return (ServiceResultCode.InvalidArguments, 0);
        }

        return (ServiceResultCode.Success, newBlog.Id);
    }
}
