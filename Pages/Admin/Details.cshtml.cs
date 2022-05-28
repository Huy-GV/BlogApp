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
using RazorBlog.Data.DTOs;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog.Pages.Admin;

[Authorize(Roles = "admin")]
public class DetailsModel : BasePageModel<DetailsModel>
{
    private readonly IUserModerationService _userModerationService;

    public DetailsModel(RazorBlogDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DetailsModel> logger,
        IUserModerationService userUserModerationService) : base(context, userManager, logger)
    {
        _userModerationService = userUserModerationService;
    }

    [BindProperty] public BanTicket? BanTicket { get; set; }
    [BindProperty] public string UserName { get; set; }
    [BindProperty] public List<HiddenCommentDto> HiddenComments { get; set; }
    [BindProperty] public List<HiddenBlogDto> HiddenBlogs { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userName)
    {
        if (userName == null)
            return NotFound();

        var user = await UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            Logger.LogInformation("User not found");
            return NotFound();
        }

        UserName = userName;
        HiddenComments = await GetHiddenComments(userName);
        HiddenBlogs = await GetHiddenBlogs(userName);
        BanTicket = await _userModerationService.FindAsync(userName);

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
                CreatedDate = b.Date,
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
                CreatedDate = c.Date,
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostBanUserAsync()
    {
        if (await UserManager.FindByNameAsync(BanTicket.UserName) == null)
        {
            return BadRequest("User not found");
        }

        if (!await _userModerationService.BanTicketExistsAsync(BanTicket.UserName))
        {
            DbContext.BanTicket.Add(BanTicket);
            await DbContext.SaveChangesAsync();
        }
        else
        {
            Logger.LogInformation("User has already been suspended");
        }

        return RedirectToPage("Details", new { username = BanTicket.UserName });
    }

    public async Task<IActionResult> OnPostLiftBanAsync(string username)
    {
        if (!await _userModerationService.BanTicketExistsAsync(username))
        {
            return BadRequest();
        }

        var banTicket = await DbContext.BanTicket
            .SingleOrDefaultAsync(s => s.UserName == username);
        await _userModerationService.RemoveBanTicketAsync(banTicket);

        return RedirectToPage("Details", new { username });
    }

    public async Task<IActionResult> OnPostUnhideBlogAsync(int blogID)
    {
        var blog = await DbContext.Blog
            .Include(b => b.AppUser)
            .SingleOrDefaultAsync(b => b.Id == blogID);

        if (blog == null)
        {
            Logger.LogError("blog not found");
            return NotFound();
        }

        DbContext.Blog.Update(blog);
        blog.IsHidden = false;
        await DbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { username = blog.AppUser.UserName });
    }

    public async Task<IActionResult> OnPostUnhideCommentAsync(int commentID)
    {
        var comment = await DbContext.Comment
            .Include(c => c.AppUser)
            .SingleOrDefaultAsync(c => c.Id == commentID);

        if (comment == null)
        {
            Logger.LogError("Comment not found");
            return NotFound();
        }

        DbContext.Comment.Update(comment);
        comment.IsHidden = false;
        await DbContext.SaveChangesAsync();

        return RedirectToPage("Details", new { username = comment.AppUser.UserName });
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentID)
    {
        var comment = await DbContext.Comment
            .Include(c => c.AppUser)
            .SingleOrDefaultAsync(c => c.Id == commentID);

        if (comment == null)
        {
            Logger.LogError("Comment not found");
            return NotFound();
        }

        DbContext.Comment.Update(comment);
        comment.IsHidden = false;
        comment.Content = "[Removed]";
        await DbContext.SaveChangesAsync();
        return RedirectToPage("Details", new { username = comment.AppUser.UserName });
    }

    public async Task<IActionResult> OnPostDeleteBlogAsync(int blogID)
    {
        var blog = await DbContext.Blog
            .Include(b => b.AppUser)
            .SingleOrDefaultAsync(b => b.Id == blogID);

        if (blog == null)
        {
            Logger.LogError("blog not found");
            return NotFound();
        }

        DbContext.Blog.Update(blog);
        blog.IsHidden = false;
        blog.Content = "[Removed]";
        await DbContext.SaveChangesAsync();
        return RedirectToPage("Details", new { username = blog.AppUser.UserName });
    }
}