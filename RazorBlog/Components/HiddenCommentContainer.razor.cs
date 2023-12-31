﻿using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Data;
using RazorBlog.Data.Dtos;
using RazorBlog.Extensions;
using RazorBlog.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorBlog.Components;

public partial class HiddenCommentContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public IDbContextFactory<RazorBlogDbContext> DbContextFactory { get; set; } = null!;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    public IReadOnlyCollection<HiddenCommentDto> HiddenComments { get; private set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadHiddenComments();
    }

    private async Task LoadHiddenComments()
    {
        HiddenComments = await GetHiddenComments(UserName);
    }

    private async Task<IReadOnlyCollection<HiddenCommentDto>> GetHiddenComments(string userName)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();   
        return await dbContext.Comment
            .Include(c => c.AppUser)
            .Where(c => c.AppUser.UserName == userName && c.IsHidden)
            .Select(c => new HiddenCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreationTime = c.CreationTime,
            })
            .ToListAsync();
    }

    private async Task ForciblyDeleteCommentAsync(int commentId)
    {
        var result = await PostModerationService.ForciblyDeleteCommentAsync(commentId, CurrentUserName);
        this.NavigateOnError(result);

        await LoadHiddenComments();
    }

    private async Task UnhideCommentAsync(int commentId)
    {
        var result = await PostModerationService.UnhideCommentAsync(commentId, CurrentUserName);
        this.NavigateOnError(result);

        await LoadHiddenComments();
    }
}
