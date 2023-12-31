using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorBlog.Components;
public partial class CommentsContainer : RichComponentBase
{
    [Parameter]
    public string BlogAuthorName { get; set; } = string.Empty;

    [Parameter]
    public int BlogId { get; set; }

    [Parameter]
    public CurrentUserInfo CurrentUserInfo { get; set; } = null!;

    [SupplyParameterFromForm]
    public CommentViewModel CreateCommentViewModel { get; set; } = new();

    [SupplyParameterFromForm]
    public CommentViewModel EditCommentViewModel { get; set; } = new();

    [Inject]
    public IDbContextFactory<RazorBlogDbContext> DbContextFactory { get; set; } = null!;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    [Inject]
    public IUserModerationService UserModerationService { get; set; } = null!;

    public bool AreCommentsLoaded { get; private set; } = false;

    public IReadOnlyCollection<CommentDto> CommentDtos { get; private set; } = [];

    public IDictionary<int, bool> IsCommentEditorDisplayed { get; private set; } = new Dictionary<int, bool>();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadCommentData();

        EditCommentViewModel.BlogId = BlogId;
        CreateCommentViewModel.BlogId = BlogId;
    }

    private async Task LoadCommentData()
    {
        CommentDtos = await LoadComments();
        IsCommentEditorDisplayed = CommentDtos
            .Where(x => x.AuthorName == CurrentUser.Identity?.Name)
            .ToDictionary(x => x.Id, _ => false);

        AreCommentsLoaded = true;
    }

    private async Task<List<CommentDto>> LoadComments()
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return await dbContext.Comment
            .Where(x => x.BlogId == BlogId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                CreationTime = c.CreationTime,
                LastUpdateTime = c.LastUpdateTime,
                Content = c.IsHidden ? ReplacementText.HiddenContent : c.Content,
                AuthorName = c.AppUser == null
                    ? ReplacementText.DeletedUser
                    : c.AppUser.UserName ?? ReplacementText.DeletedUser,
                AuthorProfileImageUri = c.AppUser == null
                    ? "readonly/default.jpg"
                    : c.AppUser.ProfileImageUri ?? "readonly/default.jpg",
                IsHidden = c.IsHidden,
                IsDeleted = c.ToBeDeleted,
            })
            .OrderByDescending(x => x.CreationTime)
            .ThenByDescending(x => x.LastUpdateTime)
            .ToListAsync();
    }

    public async Task EditCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        var user = await UserManager.GetUserAsync(base.CurrentUser);
        if (user == null || user.UserName == null)
        {
            NavigateToForbid();
            return;
        }

        if (await UserModerationService.BanTicketExistsAsync(user.UserName))
        {
            NavigateToForbid();
            return;
        }

        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var comment = await dbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            NavigateToBadRequest();
            return;
        }

        if (user.UserName != comment?.AppUser.UserName)
        {
            NavigateToForbid();
            return;
        }

        dbContext.Comment.Update(comment);
        comment.LastUpdateTime = DateTime.UtcNow;
        comment.Content = EditCommentViewModel.Content;
        await dbContext.SaveChangesAsync();
        EditCommentViewModel.Content = string.Empty;

        await LoadCommentData();
    }

    public async Task CreateCommentAsync()
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        var user = await UserManager.GetUserAsync(base.CurrentUser);
        if (user == null)
        {
            NavigateToForbid();
            return;
        }

        var userName = user.UserName ?? string.Empty;

        if (await UserModerationService.BanTicketExistsAsync(userName))
        {
            NavigateToForbid();
            return;
        }

        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var creationTime = DateTime.UtcNow;
        dbContext.Comment.Add(new Comment
        {
            AppUserId = user.Id,
            BlogId = BlogId,
            Content = CreateCommentViewModel.Content,
            CreationTime = creationTime,
            LastUpdateTime = creationTime,
        });

        await dbContext.SaveChangesAsync();

        CreateCommentViewModel.Content = string.Empty;

        await LoadCommentData();
    }

    public async Task HideCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        var user = await UserManager.GetUserAsync(base.CurrentUser);
        if (user == null)
        {
            NavigateToForbid();
            return;
        }

        var result = await PostModerationService.HideCommentAsync(commentId, user.UserName ?? string.Empty);
        this.NavigateOnError(result);

        await LoadCommentData();
    }

    public async Task DeleteCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var comment = await dbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            NavigateToNotFound();
            return;
        }

        var user = await UserManager.GetUserAsync(base.CurrentUser);
        if (user == null || user.UserName == null || user.UserName != comment.AppUser.UserName)
        {
            NavigateToForbid();
            return; 
        }

        dbContext.Comment.Remove(comment);
        await dbContext.SaveChangesAsync();

        await LoadCommentData();
    }
}
