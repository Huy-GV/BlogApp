using Microsoft.AspNetCore.Components;
using SimpleForum.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleForum.Core.Communication;
using SimpleForum.Web.Extensions;
using SimpleForum.Core.QueryServices;
using SimpleForum.Core.CommandServices;

namespace SimpleForum.Web.Components.Pages.Admin;

public partial class HiddenCommentContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    [Inject]
    public ICommentReader CommentReader { get; set; } = null!;

    private IReadOnlyCollection<ReportedCommentDto> ReportedComments { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadReportedComments();
    }

    private async Task LoadReportedComments()
    {
        var (result, reportedComments) = await CommentReader.GetReportedCommentsAsync(UserName, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        ReportedComments = reportedComments;
    }

    private async Task ForciblyDeleteCommentAsync(int commentId)
    {
        var result = await PostModerationService.ForciblyDeleteCommentAsync(commentId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadReportedComments();
    }

    private async Task UnhideCommentAsync(int reportTicketId)
    {
        var result = await PostModerationService.CancelReportTicket(reportTicketId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadReportedComments();
    }
}
