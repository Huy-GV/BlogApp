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
            .Include(blog => blog.Comments)
            .ThenInclude(comment => comment.AppUser)
            .FirstOrDefaultAsync(blog => blog.Id == id);

        if (blog == null)
        {
            return NotFound();
        }

        var blogAuthor = new
        {
            UserName = blog.AppUser?.UserName ?? ReplacementText.DeletedUser,
            // todo: un-hardcode default profile pic
            ProfileImageUri = blog.AppUser?.ProfileImageUri ?? "default.jpg",
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
            CommentDtos = blog.Comments
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    CreationTime = c.CreationTime,
                    LastUpdateTime = c.LastUpdateTime,
                    Content = c.IsHidden ? ReplacementText.HiddenContent : c.Content,
                    AuthorName = c.AppUser?.UserName ?? ReplacementText.DeletedUser,
                    AuthorProfileImageUri = c.AppUser?.ProfileImageUri ?? "default.jpg",
                    IsHidden = c.IsHidden
                })
                .ToList()
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

    public async Task<IActionResult> OnPostCreateCommentAsync()
    {
        if (!this.IsUserAuthenticated())
        {
            return Challenge();
        }

        var errorKeys = ModelState
            .Where(x => x.Value?.Errors.Any() ?? false)
            .Select(x => x.Key)
            .Distinct();

        if (errorKeys.Any(e => e.Contains(nameof(CreateCommentViewModel))))
        {
            Logger.LogError("Model state invalid when submitting new comment.");
            return RedirectToPage("/Blogs/Read", new { id = CreateCommentViewModel.BlogId });
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        var userName = user.UserName ?? string.Empty;

        if (await _userModerationService.BanTicketExistsAsync(userName))
        {
            return Forbid();
        }

        DbContext.Comment.Add(new Comment
        {
            AppUserId = user.Id,
            BlogId = CreateCommentViewModel.BlogId,
            Content = CreateCommentViewModel.Content
        });

        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Read", new { id = CreateCommentViewModel.BlogId });
    }

    public async Task<IActionResult> OnPostEditCommentAsync(int commentId)
    {
        if (!this.IsUserAuthenticated())
        {
            return Challenge();
        }

        var errorKeys = ModelState
            .Where(x => x.Value?.Errors.Any() ?? false)
            .Select(x => x.Key)
            .Distinct();

        if (errorKeys.Any(e => e.Contains(nameof(EditCommentViewModel))))
        {
            return BadRequest();
        }

        var user = await GetUserOrDefaultAsync();
        if (user == null)
        {
            return Forbid();
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName ?? string.Empty))
        {
            return Forbid();
        }

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        if (user.UserName != comment?.AppUser.UserName)
        {
            return Forbid();
        }

        comment.LastUpdateTime = DateTime.UtcNow;
        DbContext.Comment.Update(comment).CurrentValues.SetValues(EditCommentViewModel);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Read", new { id = comment.BlogId });
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

    public async Task<IActionResult> OnPostHideCommentAsync(int commentId)
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

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        if (await UserManager.IsInRoleAsync(comment.AppUser, Roles.AdminRole))
        {
            return Forbid();
        }

        await _userModerationService.HideCommentAsync(commentId);
        return RedirectToPage("/Blogs/Read", new { id = comment.BlogId });
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

        if (User.Identity?.Name != blog.AppUser.UserName)
        {
            return Forbid();
        }

        if (await _userModerationService.BanTicketExistsAsync(User.Identity?.Name))
        {
            return Forbid();
        }

        DbContext.Blog.Remove(blog);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Index");
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        if (!this.IsUserAuthenticated())
        {
            return Challenge();
        }

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return NotFound();
        }

        if (User.Identity?.Name != comment.AppUser.UserName)
        {
            return Forbid();
        }

        if (await _userModerationService.BanTicketExistsAsync(User.Identity?.Name))
        {
            return Forbid();
        }

        DbContext.Comment.Remove(comment);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Read", new { id = comment.BlogId });
    }
}