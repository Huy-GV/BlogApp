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

    private IReadOnlyCollection<HiddenThreadDto> HiddenThreads { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadHiddenThreads();
    }

    private async Task LoadHiddenThreads()
    {
        var (result, hiddenThreads) = await ThreadReader.GetHiddenThreadsAsync(UserName, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        HiddenThreads = hiddenThreads!;
    }

    private async Task ForciblyDeleteThreadAsync(int threadId)
    {
        var result = await PostModerationService.ForciblyDeleteThreadAsync(threadId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadHiddenThreads();
    }

    private async Task UnhideThreadAsync(int threadId)
    {
        var result = await PostModerationService.UnhideThreadAsync(threadId, CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadHiddenThreads();
    }
}
