using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Data.ViewModels;
using RazorBlog.Extensions;
using RazorBlog.Models;
using RazorBlog.Services;
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
    public IUserPermissionValidator UserPermissionValidator { get; set; } = null!;

    [Inject]
    public ICommentContentManager CommentContentManager { get; set; } = null!;

    private bool AreCommentsLoaded { get; set; } = false;

    private IReadOnlyCollection<CommentDto> CommentDtos { get; set; } = [];

    private IDictionary<int, bool> IsCommentEditorDisplayed { get; set; } = new Dictionary<int, bool>();

    private IReadOnlyDictionary<int, bool> AllowedToModifyComment { get; set; } = new Dictionary<int, bool>();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadCommentData();

        EditCommentViewModel.BlogId = BlogId;
        CreateCommentViewModel.BlogId = BlogId;
    }

    private async Task LoadCommentData()
    {
        var comments = await LoadComments();

        AllowedToModifyComment = await UserPermissionValidator.IsUserAllowedToUpdateOrDeletePostsAsync(
            CurrentUserName,
            comments);

        CommentDtos = comments
            .Select(c => new CommentDto
            {
                Id = c.Id,
                CreationTime = c.CreationTime,
                LastUpdateTime = c.LastUpdateTime,
                Content = c.IsHidden ? ReplacementText.HiddenContent : c.Body,
                AuthorName = c.AuthorUser == null
                    ? ReplacementText.DeletedUser
                    : c.AuthorUser.UserName ?? ReplacementText.DeletedUser,
                AuthorProfileImageUri = c.AuthorUser == null
                    ? "readonly/default.jpg"
                    : c.AuthorUser.ProfileImageUri ?? "readonly/default.jpg",
                IsHidden = c.IsHidden,
                IsDeleted = c.ToBeDeleted,
            })
            .ToList();


        IsCommentEditorDisplayed = CommentDtos
            .Where(x => x.AuthorName == CurrentUser.Identity?.Name)
            .ToDictionary(x => x.Id, _ => false);

        AreCommentsLoaded = true;
    }

    private async Task<List<Comment>> LoadComments()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return await dbContext.Comment
            .Include(x => x.AuthorUser)
            .Where(x => x.BlogId == BlogId)
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
        if (user?.UserName == null)
        {
            NavigateToForbid();
            return;
        }

        var result = await CommentContentManager.UpdateCommentAsync(
            commentId, 
            EditCommentViewModel, 
            user.UserName);

        this.NavigateOnError(result);
        EditCommentViewModel.Body = string.Empty;
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
        if (user == null || user.UserName == null)
        {
            NavigateToForbid();
            return;
        }

        var (result, _) = await CommentContentManager.CreateCommentAsync(
            CreateCommentViewModel,
            user.UserName);

        this.NavigateOnError(result);
        CreateCommentViewModel.Body = string.Empty;

        await LoadCommentData();
    }

    private async Task HideCommentAsync(int commentId)
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

    private async Task DeleteCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        var user = await UserManager.GetUserAsync(base.CurrentUser);
        if (user?.UserName == null)
        {
            NavigateToForbid();
            return; 
        }

        var result = await CommentContentManager.DeleteCommentAsync(commentId, user.UserName);

        this.NavigateOnError(result);
        EditCommentViewModel.Body = string.Empty;

        await LoadCommentData();
    }
}
