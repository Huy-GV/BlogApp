using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.ViewModels;

namespace RazorBlog.Core.WriteServices;

public interface IUserProfileEditor
{
    Task<ServiceResultCode> EditProfileSummary(string userName, EditProfileSummaryViewModel viewModel);
    Task<ServiceResultCode> ChangeProfilePicture(string userName, EditProfilePictureViewModel viewModel);
    Task<ServiceResultCode> RemoveProfilePicture(string userName);
}
