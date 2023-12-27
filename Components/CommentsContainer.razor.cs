﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Constants;
using RazorBlog.Data.Dtos;
using RazorBlog.Data.ViewModels;
using RazorBlog.Models;
using RazorBlog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    ? "default.jpg"
                    : c.AppUser.ProfileImageUri ?? "default.jpg",
                IsHidden = c.IsHidden
            })
            .ToListAsync();
    }

    public async Task EditCommentAsync(int commentId)
    {
        if (!IsAuthenticated)
        {
            Challenge();
            return;
        }

        var user = await UserManager.GetUserAsync(User);
        if (user == null || user.UserName == null)
        {
            Forbid();
            return;
        }

        if (await UserModerationService.BanTicketExistsAsync(user.UserName))
        {
            Forbid();
            return;
        }

        var comment = await DbContext.Comment
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == commentId);

        if (comment == null)
        {
            BadRequest();
            return;
        }

        if (user.UserName != comment?.AppUser.UserName)
        {
            Forbid();
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
            Challenge();
            return;
        }

        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            Forbid();
            return;
        }

        var userName = user.UserName ?? string.Empty;

        if (await UserModerationService.BanTicketExistsAsync(userName))
        {
            Forbid();
            return;
        }

        DbContext.Comment.Add(new Comment
        {
            AppUserId = user.Id,
            BlogId = BlogId,
            Content = CreateCommentViewModel.Content,
            CreationTime = DateTime.UtcNow,
        });

        await DbContext.SaveChangesAsync();

        CreateCommentViewModel.Content = string.Empty;

        await LoadCommentData();
    }
}
