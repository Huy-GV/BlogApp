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
using RazorBlog.Data.DTOs;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Blogs;

[AllowAnonymous]
public class ReadModel : BasePageModel<ReadModel>
{
    private readonly IUserModerationService _moderationService;

    public ReadModel(
        RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ReadModel> logger,
        IUserModerationService moderationService) : base(
    context, userManager, logger)
    {
        _moderationService = moderationService;
    }

    [BindProperty] public CommentViewModel CreateCommentViewModel { get; set; }

    [BindProperty] public CommentViewModel EditCommentViewModel { get; set; }

    [BindProperty(SupportsGet = true)] public CurrentUserInfo CurrentUserInfo { get; set; }

    [BindProperty(SupportsGet = true)] public DetailedBlogDto DetailedBlogDto { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var blog = await DbContext.Blog
            .Include(blog => blog.AppUser)
            .Include(blog => blog.Comments)
            .ThenInclude(comment => comment.AppUser)
            .SingleOrDefaultAsync(blog => blog.Id == id);

        if (blog == null) return NotFound();

        var blogAuthor = new
        {
            UserName = blog.AppUser?.UserName ?? "Deleted User",
            // todo: un-hardcode default profile pic
            ProfileImageUri = blog.AppUser?.ProfileImageUri ?? "default.jpg",
            Description = blog.AppUser?.Description ?? "Deleted User"
        };

        DbContext.Blog.Update(blog);
        blog.ViewCount++;
        await DbContext.SaveChangesAsync();
        DetailedBlogDto = new DetailedBlogDto
        {
            Id = blog.Id,
            Introduction = blog.Introduction,
            Title = blog.Title,
            Content = blog.Content,
            CoverImageUri = blog.CoverImageUri,
            Date = blog.Date,
            IsHidden = blog.IsHidden,
            AuthorDescription = blogAuthor.Description,
            AuthorName = blogAuthor.UserName,
            AuthorProfileImageUri = blogAuthor.ProfileImageUri,
            CommentDtos = blog.Comments
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Date = c.Date,
                    Content = c.Content,
                    AuthorName = c.AppUser?.UserName ?? "Deleted User",
                    AuthorProfileImageUri = c.AppUser?.ProfileImageUri ?? "default.jpg",
                    IsHidden = c.IsHidden
                })
                .ToList()
        };

        var currentUser = await GetUserAsync();
        var currentUserName = currentUser?.UserName ?? string.Empty;
        var currentUserRoles = currentUser != null
            ? await UserManager.GetRolesAsync(currentUser)
            : new List<string>();

        CurrentUserInfo = new CurrentUserInfo
        {
            UserName = currentUserName,
            AllowedToHideBlogOrComment = currentUser != null &&
                                         currentUserRoles.Intersect(new[] { Roles.AdminRole, Roles.ModeratorRole })
                                             .Any(),
            AllowedToModifyOrDeleteBlog = currentUserName == DetailedBlogDto.AuthorName,
            IsBanned = currentUser != null && await _moderationService.BanTicketExistsAsync(currentUserName),
            IsAuthenticated = this.IsUserAuthenticated()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostCreateCommentAsync()
    {
        if (!this.IsUserAuthenticated()) return Challenge();

        if (!ModelState.IsValid)
        {
            Logger.LogError("Model state invalid when submitting new comment.");
            //TODO: return an appropriate response
            return NotFound();
        }

        var user = await GetUserAsync();
        var userName = user.UserName;

        if (await _moderationService.BanTicketExistsAsync(userName)) return Forbid();

        DbContext.Comment.Add(new Comment
        {
            Date = DateTime.Now,
            AppUserId = user.Id,
            BlogId = CreateCommentViewModel.BlogId,
            Content = CreateCommentViewModel.Content
        });

        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Read", new { id = CreateCommentViewModel.BlogId });
    }

    public async Task<IActionResult> OnPostEditCommentAsync(int commentId)
    {
        if (!this.IsUserAuthenticated()) return Challenge();

        if (!ModelState.IsValid)
        {
            Logger.LogError("Invalid model state when editting comments");
            return NotFound();
        }

        var user = await GetUserAsync();
        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .SingleOrDefaultAsync(x => x.Id == commentId);

        if (user.UserName != comment.AppUser.UserName) return Forbid();

        DbContext.Comment.Update(comment).CurrentValues.SetValues(EditCommentViewModel);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Read", new { id = comment.BlogId });
    }

    public async Task<IActionResult> OnPostHideBlogAsync(int blogId)
    {
        if (!this.IsUserAuthenticated()) return Challenge();

        var user = await GetUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        if (!roles.Contains(Roles.AdminRole) && !roles.Contains(Roles.ModeratorRole)) return Forbid();

        var blog = await DbContext.Blog
            .Include(x => x.AppUser)
            .SingleOrDefaultAsync(x => x.Id == blogId);

        if (blog == null) return NotFound();

        if (await UserManager.IsInRoleAsync(blog.AppUser, Roles.AdminRole)) return Forbid();
        
        await _moderationService.HideBlogAsync(blogId);
        return RedirectToPage("/Blogs/Read", new { id = blogId });
    }

    public async Task<IActionResult> OnPostHideCommentAsync(int commentId)
    {
        if (!this.IsUserAuthenticated()) return Challenge();

        var user = await GetUserAsync();
        var roles = await UserManager.GetRolesAsync(user);
        if (!roles.Contains(Roles.AdminRole) && !roles.Contains(Roles.ModeratorRole)) return Forbid();

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .SingleOrDefaultAsync(x => x.Id == commentId);

        if (comment == null) return NotFound();

        if (await UserManager.IsInRoleAsync(comment.AppUser, Roles.AdminRole)) return Forbid();

        // comment.SuspensionExplanation = Messages.InappropriateComment;
        // await DbContext.SaveChangesAsync();

        await _moderationService.HideCommentAsync(commentId);
        return RedirectToPage("/Blogs/Read", new { id = comment.BlogId });
    }

    public async Task<IActionResult> OnPostDeleteBlogAsync(int blogId)
    {
        if (!this.IsUserAuthenticated()) return Challenge();

        var blog = await DbContext.Blog
            .Include(x => x.AppUser)
            .SingleOrDefaultAsync(x => x.Id == blogId);

        // todo: check identity name and user.UserName
        if (User.Identity?.Name != blog.AppUser.UserName) return Forbid();

        if (blog == null) return NotFound();

        DbContext.Blog.Remove(blog);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Index");
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        if (!this.IsUserAuthenticated()) return Challenge();

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .SingleOrDefaultAsync(x => x.Id == commentId);

        if (comment == null) return NotFound();

        if (User.Identity?.Name != comment.AppUser.UserName) return Forbid();

        DbContext.Comment.Remove(comment);
        await DbContext.SaveChangesAsync();

        return RedirectToPage("/Blogs/Read", new { id = comment.BlogId });
    }
}