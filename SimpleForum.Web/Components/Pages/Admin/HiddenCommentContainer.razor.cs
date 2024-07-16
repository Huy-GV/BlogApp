using Microsoft.AspNetCore.Components;
using SimpleForum.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleForum.Core.Communication;
using SimpleForum.Web.Extensions;
using SimpleForum.Core.WriteServices;
using SimpleForum.Core.ReadServices;

namespace SimpleForum.Web.Components.Pages.Admin;

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
