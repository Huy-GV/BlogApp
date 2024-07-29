using System.Threading.Tasks;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.ViewModels;

namespace SimpleForum.Core.CommandServices;

public interface IUserProfileEditor
{
    Task<ServiceResultCode> EditProfileSummary(string userName, EditProfileSummaryViewModel viewModel);
    Task<ServiceResultCode> ChangeProfilePicture(string userName, EditProfilePictureViewModel viewModel);
    Task<ServiceResultCode> RemoveProfilePicture(string userName);
}
