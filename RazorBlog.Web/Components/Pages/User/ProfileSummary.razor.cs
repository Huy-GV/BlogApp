using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data.Dtos;
using RazorBlog.Core.Data.ViewModels;
using RazorBlog.Core.ReadServices;
using RazorBlog.Core.WriteServices;
using RazorBlog.Web.Extensions;

namespace RazorBlog.Web.Components.Pages.User;
public partial class ProfileSummary : RichComponentBase
{
    public PersonalProfileDto UserProfile { get; set; } = new();

    [SupplyParameterFromForm]
    public EditProfileSummaryViewModel EditProfileSummaryViewModel { get; set; } = new();

    [SupplyParameterFromForm]
    public EditProfilePictureViewModel EditProfilePictureViewModel { get; set; } = new();

    [Inject]
    public IUserProfileReader UserProfileReader { get; set; } = null!;

    [Inject]
    public IUserProfileEditor UserProfileEditor { get; set; } = null!;

    private bool IsSummaryEditFormDisplayed { get; set; } = false;
    private bool AreProfilePictureOptionsDisplayed { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadUserProfile();
    }

    private async Task LoadUserProfile()
    {
        var (result, profile) = await UserProfileReader.GetUserProfileAsync(CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        UserProfile = profile!;
    }

    public async Task EditProfileSummaryAsync()
    {
        var result = await UserProfileEditor.EditProfileSummary(CurrentUserName, EditProfileSummaryViewModel);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadUserProfile();
        HideSummaryEditorForm();
    }

    public async Task EditProfilePictureAsync()
    {
        var result = await UserProfileEditor.ChangeProfilePicture(CurrentUserName, EditProfilePictureViewModel);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadUserProfile();
    }

    public async Task RemoveProfilePictureAsync()
    {
        var result = await UserProfileEditor.RemoveProfilePicture(CurrentUserName);
        if (result != ServiceResultCode.Success)
        {
            this.NavigateOnError(result);
            return;
        }

        await LoadUserProfile();
    }
}
