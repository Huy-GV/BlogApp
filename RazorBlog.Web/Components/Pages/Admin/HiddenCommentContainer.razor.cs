using Microsoft.AspNetCore.Components;
using RazorBlog.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using RazorBlog.Core.Communication;
using RazorBlog.Web.Extensions;
using RazorBlog.Core.WriteServices;
using RazorBlog.Core.ReadServices;

namespace RazorBlog.Web.Components.Pages.Admin;

public partial class HiddenCommentContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    [Inject]
    public ICommentReader CommentReader { get; set; } = null!;

    private IReadOnlyCollection<HiddenCommentDto> HiddenComments { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadHiddenComments();
    }

    private async Task LoadHiddenComments()
    {
        var (result, hiddenComments) = await CommentReader.GetHiddenCommentsAsync(UserName, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        HiddenComments = hiddenComments;
    }

    private async Task ForciblyDeleteCommentAsync(int commentId)
    {
        var result = await PostModerationService.ForciblyDeleteCommentAsync(commentId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadHiddenComments();
    }

    private async Task UnhideCommentAsync(int commentId)
    {
        var result = await PostModerationService.UnhideCommentAsync(commentId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadHiddenComments();
    }
}
