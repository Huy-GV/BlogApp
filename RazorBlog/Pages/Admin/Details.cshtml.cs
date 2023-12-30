using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using RazorBlog.Data.Validation;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Admin;

[Authorize(Roles = Roles.AdminRole)]
public class DetailsModel(
    RazorBlogDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<DetailsModel> logger,
    IUserModerationService userUserModerationService,
    IPostModerationService postModerationService,
    IPostDeletionScheduler postDeletionService) : BasePageModel<DetailsModel>(context, userManager, logger)
{
    private readonly IUserModerationService _userModerationService = userUserModerationService;
    private readonly IPostDeletionScheduler _postDeletionService = postDeletionService;
    private readonly IPostModerationService _postModerationService = postModerationService;

    [BindProperty(SupportsGet =true)] 
    public BanTicket? CurrentBanTicket { get; set; }

    [BindProperty]
    [DateRange(allowsPast: false, allowsFuture: true, ErrorMessage ="Expiry date must be in the future")]
    public DateTime NewBanTicketExpiryDate { get; set; } = DateTime.Now.AddDays(1);

    [BindProperty(SupportsGet = true)]
    [Required]
    public string UserName { get; set; } = string.Empty;

    [BindProperty] 
    public List<HiddenCommentDto> HiddenComments { get; set; } = [];

    [BindProperty] 
    public List<HiddenBlogDto> HiddenBlogs { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string? userName)
    {
        if (userName == null)
        {
            return NotFound();
        }

        var user = await UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            Logger.LogInformation("User not found");
            return NotFound();
        }

        UserName = userName;
        HiddenComments = await GetHiddenComments(userName);
        HiddenBlogs = await GetHiddenBlogs(userName);
        CurrentBanTicket = await _userModerationService.FindBanTicketByUserNameAsync(userName);

        return Page();
    }

    private async Task<List<HiddenBlogDto>> GetHiddenBlogs(string username)
    {
        return await DbContext.Blog
            .Include(b => b.AppUser)
            .Where(b => b.AppUser.UserName == username && b.IsHidden)
            .Select(b => new HiddenBlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Introduction = b.Introduction,
                Content = b.Content,
                CreationTime = b.CreationTime,
            })
            .ToListAsync();
    }

    private async Task<List<HiddenCommentDto>> GetHiddenComments(string username)
    {
        return await DbContext.Comment
            .Include(c => c.AppUser)
            .Where(c => c.AppUser.UserName == username && c.IsHidden)
            .Select(c => new HiddenCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreationTime = c.CreationTime,
            })
            .ToListAsync();
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

    public async Task<IActionResult> OnPostBanUserAsync()
    {
        if (!ValidatorUtil.TryValidateProperty(NewBanTicketExpiryDate, nameof(NewBanTicketExpiryDate), this))
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(UserName) || await UserManager.FindByNameAsync(UserName) == null)
        {
            return BadRequest("User not found");
        }

        await _userModerationService.BanUserAsync(UserName, User.Identity?.Name ?? string.Empty, NewBanTicketExpiryDate);

        return RedirectToPage("Details", new { userName = UserName });
    }

    public async Task<IActionResult> OnPostLiftBanAsync(string userName)
    {
        if (!await _userModerationService.BanTicketExistsAsync(userName))
        {
            return BadRequest();
        }

        await _userModerationService.RemoveBanTicketAsync(userName, User.Identity?.Name ?? string.Empty);

        return RedirectToPage("Details", new { userName });
    }

    public async Task<IActionResult> OnPostUnhideBlog(int blogId)
    {
        if (!ValidatorUtil.TryValidateProperty(UserName, nameof(UserName), this))
        {
            return Page();
        }

        var result = await _postModerationService.UnhideBlogAsync(blogId, User.Identity?.Name ?? string.Empty);

        return RedirectToPage("Details", new { userName = UserName });
    }

    public async Task<IActionResult> OnPostUnhideCommentAsync(int commentId)
    {
        if (!ValidatorUtil.TryValidateProperty(UserName, nameof(UserName), this))
        {
            return Page();
        }

        var result = await _postModerationService.UnhideCommentAsync(commentId, User.Identity?.Name ?? string.Empty);

        return RedirectToPage("Details", new { userName = UserName });
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        var comment = await DbContext.Comment
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            Logger.LogError("Comment not found");
            return NotFound();
        }

        DbContext.Comment.Update(comment);
        CensorDeletedComment(comment);
        await DbContext.SaveChangesAsync();
        var deleteTime = new DateTimeOffset(DateTime.UtcNow.AddDays(7));
        _postDeletionService.ScheduleBlogDeletion(deleteTime, comment.Id);

        return RedirectToPage("Details", new { userName = UserName });
    }

    public async Task<IActionResult> OnPostDeleteBlogAsync(int blogId)
    {
        var blog = await DbContext.Blog
            .FirstOrDefaultAsync(b => b.Id == blogId);

        if (blog == null)
        {
            Logger.LogError("Blog not found");
            return NotFound();
        }

        DbContext.Blog.Update(blog);
        CensorDeletedBlog(blog);
        await DbContext.SaveChangesAsync();

        var deleteTime = new DateTimeOffset(DateTime.UtcNow.AddDays(14));
        _postDeletionService.ScheduleBlogDeletion(deleteTime, blog.Id);
        return RedirectToPage("Details", new { userName = UserName });
    }
}