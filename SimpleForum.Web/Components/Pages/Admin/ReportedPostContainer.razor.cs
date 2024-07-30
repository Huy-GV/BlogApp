using Microsoft.AspNetCore.Components;

namespace SimpleForum.Web.Components.Pages.Admin;

public partial class ReportedPostContainer : RichComponentBase
{
    [Parameter]
    public string UserName { get; set; } = string.Empty;

    private bool ReloadToggled { get; set; }

    private void Reload()
    {
        ReloadToggled = !ReloadToggled;
    }
}
