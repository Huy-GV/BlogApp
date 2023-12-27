using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorBlog.Components;

public partial class CommentsContainer : ComponentBase
{
    [Parameter]
    public string BlogAuthorName { get; set; } = string.Empty;

    [Parameter]
    public int BlogId { get; set; }

    [Parameter]
    public CurrentUserInfo CurrentUser { get; set; } = null!;

    public IReadOnlyCollection<CommentDto> CommentDtos { get; private set; } = [];

    public IDictionary<int, bool> IsCommentEditorDisplayed { get; private set; } = new Dictionary<int, bool>();

    [SupplyParameterFromForm]
    public CommentViewModel CreateCommentViewModel { get; set; } = new();

    [SupplyParameterFromForm]
    public CommentViewModel EditCommentViewModel { get; set; } = new();

    private ClaimsPrincipal _userClaimsPrincipal = new();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        // TODO: set comments here, comments should be re rendered every time but user principal should only be rendered once
        _userClaimsPrincipal = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;


        await LoadCommentData();

        EditCommentViewModel.BlogId = BlogId;
        CreateCommentViewModel.BlogId = BlogId;
    }

    private async Task LoadCommentData()
    {
        CommentDtos = await LoadComments();
        IsCommentEditorDisplayed = LoadCommentEditorStatus();
    }

    private bool IsUserAuthenticated()
    {
        return _userClaimsPrincipal?.Identity?.IsAuthenticated ?? false;
    }

    private IDictionary<int, bool> LoadCommentEditorStatus()
    {
        return CommentDtos
            .Where(x => x.AuthorName == _userClaimsPrincipal.Identity?.Name)
            .ToDictionary(x => x.Id, _ => false);
    }

    private async Task<List<CommentDto>> LoadComments()
    {
        return await _dbContext.Comment
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
                    ? "default.jpg"
                    : c.AppUser.ProfileImageUri ?? "default.jpg",
                IsHidden = c.IsHidden
            })
            .ToListAsync();
    }

    public async Task EditCommentAsync(int commentId)
    {
        // TODO: validate form

        // TODO: save model
        if (!this.IsUserAuthenticated())
        {
            return;
        }

        var user = await _userManager.GetUserAsync(_userClaimsPrincipal ?? new());
        if (user == null || user.UserName == null)
        {
            return;
        }

        if (await _userModerationService.BanTicketExistsAsync(user.UserName))
        {
            return;
        }

        var comment = await _dbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            return;
        }

        if (user.UserName != comment?.AppUser.UserName)
        {
            return;
        }

        comment.LastUpdateTime = DateTime.UtcNow;
        _dbContext.Comment.Update(comment).CurrentValues.SetValues(EditCommentViewModel);
        await _dbContext.SaveChangesAsync();

        await LoadCommentData();
    }

    public async Task CreateCommentAsync()
    {
        if (!IsUserAuthenticated())
        {
            return;
        }

        // TODO: rewrite form validation here
        //var errorKeys = ModelState
        //    .Where(x => x.Value?.Errors.Any() ?? false)
        //    .Select(x => x.Key)
        //    .Distinct();

        //if (errorKeys.Any(e => e.Contains(nameof(CreateCommentViewModel))))
        //{
        //    _logger.LogError("Model state invalid when submitting new comment.");
        //    return;
        //}

        var user = await _userManager.GetUserAsync(_userClaimsPrincipal ?? new());
        if (user == null)
        {
            return;
        }

        var userName = user.UserName ?? string.Empty;

        if (await _userModerationService.BanTicketExistsAsync(userName))
        {
            return;
        }

        _dbContext.Comment.Add(new Comment
        {
            AppUserId = user.Id,
            BlogId = BlogId,
            Content = CreateCommentViewModel.Content
        });

        await _dbContext.SaveChangesAsync();

        CreateCommentViewModel.Content = string.Empty;

        await LoadCommentData();
    }
}
