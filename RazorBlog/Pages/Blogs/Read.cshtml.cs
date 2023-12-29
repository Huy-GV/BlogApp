using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class ReadModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<ReadModel> logger,
    IUserModerationService moderationService) : BasePageModel<ReadModel>(
context, userManager, logger)
{
    private readonly IUserModerationService _userModerationService = moderationService;

    [BindProperty]
    public CommentViewModel CreateCommentViewModel { get; set; } = null!;

    [BindProperty]
    public CommentViewModel EditCommentViewModel { get; set; } = null!;

    [BindProperty(SupportsGet = true)] 
    public CurrentUserInfo CurrentUserInfo { get; set; } = null!;

    [BindProperty(SupportsGet = true)] 
    public DetailedBlogDto DetailedBlogDto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await DbContext.Blog
            .IgnoreQueryFilters()
            .Include(blog => blog.AppUser)
            .FirstOrDefaultAsync(blog => blog.Id == id);

        if (blog == null)
        {
            return NotFound();
        }

        var blogAuthor = new
        {
            UserName = blog.AppUser?.UserName ?? ReplacementText.DeletedUser,
            // todo: un-hardcode default profile pic
            ProfileImageUri = blog.AppUser?.ProfileImageUri ?? "readonly/default.jpg",
            Description = blog.AppUser?.Description ?? ReplacementText.DeletedUser
        };

        DbContext.Blog.Update(blog);
        blog.ViewCount++;
        await DbContext.SaveChangesAsync();
        DetailedBlogDto = new DetailedBlogDto
        {
            Id = blog.Id,
            Introduction = blog.IsHidden ? ReplacementText.HiddenContent : blog.Introduction,
            Title = blog.IsHidden ? ReplacementText.HiddenContent : blog.Title,
            Content = blog.IsHidden ? ReplacementText.HiddenContent : blog.Content,
            CoverImageUri = blog.CoverImageUri,
            CreationTime = blog.CreationTime,
            LastUpdateTime = blog.LastUpdateTime,
            IsHidden = blog.IsHidden,
            AuthorDescription = blogAuthor.Description,
            AuthorName = blogAuthor.UserName,
            AuthorProfileImageUri = blogAuthor.ProfileImageUri,
        };

        var currentUser = await GetUserOrDefaultAsync();
        var currentUserName = currentUser?.UserName ?? string.Empty;
        var currentUserRoles = currentUser != null
            ? await UserManager.GetRolesAsync(currentUser)
            : new List<string>();

        CurrentUserInfo = new CurrentUserInfo
        {
            UserName = currentUserName,
            AllowedToHideBlogOrComment = currentUser != null &&
                                         currentUserRoles
                                            .Intersect(new[] { Roles.AdminRole, Roles.ModeratorRole })
                                            .Any(),
            AllowedToModifyOrDeleteBlog = currentUserName == DetailedBlogDto.AuthorName,
            IsBanned = currentUser != null && await _userModerationService.BanTicketExistsAsync(currentUserName),
            IsAuthenticated = this.IsUserAuthenticated()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostHideBlogAsync(int blogId)
    {
        if (!this.IsUserAuthenticated())
        {
            return Challenge();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        var roles = await UserManager.GetRolesAsync(user);
        if (!roles.Contains(Roles.AdminRole) && !roles.Contains(Roles.ModeratorRole))
        {
            return Forbid();
        }

        var blog = await DbContext.Blog
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            return NotFound();
        }

        if (await UserManager.IsInRoleAsync(blog.AppUser, Roles.AdminRole))
        {
            return Forbid();
        }

        await _userModerationService.HideBlogAsync(blogId);
        return RedirectToPage("/Blogs/Read", new { id = blogId });
    }

    public async Task<IActionResult> OnPostDeleteBlogAsync(int blogId)
    {
        if (!this.IsUserAuthenticated())
        {
            return Challenge();
        }

        var blog = await DbContext.Blog
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == blogId);

        if (blog == null)
        {
            return NotFound();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null || user.UserName == null || user.UserName != blog.AppUser.UserName)
        {
            return Forbid();
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName))
        {
            return Forbid();
        }

        DbContext.Blog.Remove(blog);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Index");
    }
}