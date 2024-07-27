using Microsoft.AspNetCore.Components;
using SimpleForum.Core.Data.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleForum.Core.Communication;
using SimpleForum.Web.Extensions;
using SimpleForum.Core.WriteServices;
using SimpleForum.Core.ReadServices;

namespace SimpleForum.Web.Components.Pages.Admin;

public partial class HiddenThreadContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    [Inject]
    public IPostModerationService PostModerationService { get; set; } = null!;

    [Inject]
    public IThreadReader ThreadReader { get; set; } = null!;

    private IReadOnlyCollection<HiddenThreadDto> ReportedThreads { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadReportedThreads();
    }

    private async Task LoadReportedThreads()
    {
        var (result, reportedhreads) = await ThreadReader.GetReportTicketAsync(UserName, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        ReportedThreads = reportedhreads!;
    }

    private async Task ForciblyDeleteThreadAsync(int threadId)
    {
        var result = await PostModerationService.ForciblyDeleteThreadAsync(threadId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadReportedThreads();
    }

    private async Task CancelThreadReportTicket(int reportTicketId)
    {
        var result = await PostModerationService.CancelReportTicket(reportTicketId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadReportedThreads();
    }
}
