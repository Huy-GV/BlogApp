using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.ViewModels;

namespace SimpleForum.Core.WriteServices;

public interface IUserProfileEditor
{
    Task<ServiceResultCode> EditProfileSummary(string userName, EditProfileSummaryViewModel viewModel);
    Task<ServiceResultCode> ChangeProfilePicture(string userName, EditProfilePictureViewModel viewModel);
    Task<ServiceResultCode> RemoveProfilePicture(string userName);
}
