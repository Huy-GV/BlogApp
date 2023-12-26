using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

    public CommentViewModel CreateCommentViewModel { get; set; } = null!;

    private ClaimsPrincipal? _userClaimsPrincipal;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        // TODO: set comments here, comments should be re rendered every time but user principal should only be rendered once
        var userState = _authenticationStateProvider.GetAuthenticationStateAsync().Result;
        _userClaimsPrincipal = userState.User;

        CommentDtos = _dbContext.Comment
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
            .ToList();
    }

    private bool IsUserAuthenticated()
    {
        return _userClaimsPrincipal?.Identity?.IsAuthenticated ?? false;
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
    }
}
