using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public CurrentUserInfo CurrentUser { get; set; } = null!;

    [SupplyParameterFromForm]
    public CommentViewModel CreateCommentViewModel { get; set; } = new();

    [SupplyParameterFromForm]
    public CommentViewModel EditCommentViewModel { get; set; } = new();

    [Inject]
    public RazorBlogDbContext DbContext { get; set; } = null!;

    [Inject]
    public ILogger<CommentsContainer> Logger { get; set; } = null!;

    [Inject]
    public IUserModerationService UserModerationService { get; set; } = null!;

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
            .Where(x => x.AuthorName == User?.Identity?.Name)
            .ToDictionary(x => x.Id, _ => false);
    }

    private async Task<List<CommentDto>> LoadComments()
    {
        return await DbContext.Comment
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
                IsHidden = c.IsHidden
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

        var user = await UserManager.GetUserAsync(User);
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

        var comment = await DbContext.Comment
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

        DbContext.Comment.Update(comment);
        comment.LastUpdateTime = DateTime.UtcNow;
        comment.Content = EditCommentViewModel.Content;
        await DbContext.SaveChangesAsync();
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

        var user = await UserManager.GetUserAsync(User);
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

        var creationTime = DateTime.UtcNow;
        DbContext.Comment.Add(new Comment
        {
            AppUserId = user.Id,
            BlogId = BlogId,
            Content = CreateCommentViewModel.Content,
            CreationTime = creationTime,
            LastUpdateTime = creationTime,
        });

        await DbContext.SaveChangesAsync();

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

        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            NavigateToForbid();
            return;
        }

        var result = await UserModerationService.HideCommentAsync(commentId, user.Id);
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

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            NavigateToNotFound();
            return;
        }

        var user = await UserManager.GetUserAsync(User);
        if (user == null || user.UserName == null || user.UserName != comment.AppUser.UserName)
        {
            NavigateToForbid();
            return; 
        }

        DbContext.Comment.Remove(comment);
        await DbContext.SaveChangesAsync();

        await LoadCommentData();
    }
}
