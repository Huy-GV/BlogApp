using Microsoft.AspNetCore.Components;
using RazorBlog.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using RazorBlog.Core.Communication;
using RazorBlog.Web.Extensions;
using RazorBlog.Core.WriteServices;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Web.Components.Pages.Admin;

public partial class HiddenBlogContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    [Inject]
    public IBlogReader BlogReader { get; set; } = null!;

    private IReadOnlyCollection<HiddenBlogDto> HiddenBlogs { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadHiddenBlogs();
    }

    private async Task LoadHiddenBlogs()
    {
        var (result, hiddenBlogs) = await BlogReader.GetHiddenBlogsAsync(UserName, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        HiddenBlogs = hiddenBlogs!;
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
