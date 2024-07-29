using Microsoft.AspNetCore.Components;
using Microsoft.FeatureManagement;
using SimpleForum.Core.CommandServices;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.ViewModels;
using SimpleForum.Core.Features;
using SimpleForum.Core.Models;
using SimpleForum.Core.QueryServices;
using SimpleForum.Web.Extensions;
using System.Threading.Tasks;

namespace SimpleForum.Web.Components.Pages.Admin;

public partial class BanUserForm : RichComponentBase
{
    [Parameter]
    public string InspectedUserName { get; set; } = string.Empty;

    private BanTicket? CurrentBanTicket { get; set; }

    [SupplyParameterFromForm]
    private BanUserViewModel BanUserViewModel { get; set; } = new();

    [Inject]
    public IBanTicketReader BanTicketReader { get; set; } = null!;

    [Inject]
    public IUserModerationService UserModerationService { get; set; } = null!;

    [Inject]
    public IFeatureManager FeatureManager { get; set; } = null!;

    private bool IsConfirmBanButtonDisplayed { get; set; } = false;

    private bool IsTemporaryBanEnabled { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadBanTicketAsync();

        IsTemporaryBanEnabled = await FeatureManager.IsEnabledAsync(FeatureNames.UseHangFire);
    }

    private async Task LoadBanTicketAsync()
    {
        CurrentBanTicket = await BanTicketReader.FindBanTicketByUserNameAsync(InspectedUserName);
    }

    private async Task BanUserAsync()
    {
        IsConfirmBanButtonDisplayed = false;
        if (string.IsNullOrWhiteSpace(InspectedUserName) || await UserManager.FindByNameAsync(InspectedUserName) == null)
        {
            this.NavigateToBadRequest();
            return;
        }

        var result = await UserModerationService.BanUserAsync(
            InspectedUserName,
            CurrentUserName ?? string.Empty,
            BanUserViewModel.NewBanTicketExpiryDate);

        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadBanTicketAsync();
        return;
    }

    private async Task LiftBanAsync()
    {
        IsConfirmBanButtonDisplayed = false;
        if (!await BanTicketReader.BanTicketExistsAsync(InspectedUserName))
        {
            this.NavigateToBadRequest();
            return;
        }

        var result = await UserModerationService.RemoveBanTicketAsync(InspectedUserName, CurrentUserName ?? string.Empty);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadBanTicketAsync();
        return;
    }
}
