using System.Threading.Tasks;
using RazorBlog.Core.Communication;
using RazorBlog.Core.Data.Dtos;

namespace RazorBlog.Core.ReadServices;
public interface IUserProfileReader
{
    Task<(ServiceResultCode, PersonalProfileDto?)> GetUserProfileAsync(string userName);
}
