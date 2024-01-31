﻿using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Data;
using RazorBlog.Data.Dtos;
using RazorBlog.Extensions;
using RazorBlog.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazorBlog.Communication;

namespace RazorBlog.Components;

public partial class HiddenBlogContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public IDbContextFactory<RazorBlogDbContext> DbContextFactory { get; set; } = null!;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    private IReadOnlyCollection<HiddenBlogDto> HiddenBlogs { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadHiddenBlogs();
    }

    private async Task<List<HiddenBlogDto>> GetHiddenBlogs(string userName)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.Blog
            .AsNoTracking()
            .Include(b => b.AuthorUser)
            .Where(b => b.AuthorUser.UserName == userName && b.IsHidden)
            .Select(b => new HiddenBlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Introduction = b.Introduction,
                Content = b.Body,
                CreationTime = b.CreationTime,
            })
            .ToListAsync();
    }

    private async Task LoadHiddenBlogs()
    {
        HiddenBlogs = await GetHiddenBlogs(UserName);
    }

    private async Task ForciblyDeleteBlogAsync(int blogId)
    {
        var result = await PostModerationService.ForciblyDeleteBlogAsync(blogId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadHiddenBlogs();
    }

    private async Task UnhideBlogAsync(int blogId)
    {
        var result = await PostModerationService.UnhideBlogAsync(blogId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadHiddenBlogs();
    }
}
