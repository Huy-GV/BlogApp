using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Dtos;
using RazorBlog.Core.Data.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazorBlog.Core.Communication;
using RazorBlog.Web.Extensions;
using RazorBlog.Core.WriteServices;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Web.Components.Pages.Blogs;
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
    public IPostModerationService PostModerationService { get; set; } = null!;

    [Inject]
    public IUserPermissionValidator UserPermissionValidator { get; set; } = null!;

    [Inject]
    public ICommentContentManager CommentContentManager { get; set; } = null!;

    [Inject]
    public ICommentReader CommentReader { get; set; } = null!;

    private bool AreCommentsLoaded { get; set; }

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
        var comments = await CommentReader.GetCommentsAsync(BlogId);

        CommentDtos = comments;
        AllowedToModifyComment = await UserPermissionValidator.IsUserAllowedToUpdateOrDeletePostsAsync
        (
            CurrentUserName,
            comments.Select
            (
                x => new PostPermissionViewModel<int>
                {
                    PostId = x.Id,
                    AuthorUserName = x.AuthorName,
                    IsHidden = x.IsHidden
                }
            )
        );

        IsCommentEditorDisplayed = CommentDtos
            .Where(x => x.AuthorName == CurrentUser.Identity?.Name)
            .ToDictionary(x => x.Id, _ => false);

        AreCommentsLoaded = true;
    }

    public async Task EditCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        var user = await UserManager.GetUserAsync(CurrentUser);
        if (user?.UserName == null)
        {
            NavigateToForbid();
            return;
        }

        var result = await CommentContentManager.UpdateCommentAsync(
            commentId,
            EditCommentViewModel,
            user.UserName);

        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

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

        var user = await UserManager.GetUserAsync(CurrentUser);
        if (user?.UserName is null)
        {
            NavigateToForbid();
            return;
        }

        var (result, _) = await CommentContentManager.CreateCommentAsync(
            CreateCommentViewModel,
            user.UserName);

        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

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

        var user = await UserManager.GetUserAsync(CurrentUser);
        if (user == null)
        {
            NavigateToForbid();
            return;
        }

        var result = await PostModerationService.HideCommentAsync(commentId, user.UserName ?? string.Empty);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadCommentData();
    }

    private async Task DeleteCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            NavigateToChallenge();
            return;
        }

        var user = await UserManager.GetUserAsync(CurrentUser);
        if (user?.UserName == null)
        {
            NavigateToForbid();
            return;
        }

        var result = await CommentContentManager.DeleteCommentAsync(commentId, user.UserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        EditCommentViewModel.Body = string.Empty;
        await LoadCommentData();
    }
}
