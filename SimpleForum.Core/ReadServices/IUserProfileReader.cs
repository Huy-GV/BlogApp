using System.Threading.Tasks;
using SimpleForum.Core.Communication;
using SimpleForum.Core.Data.Dtos;

namespace SimpleForum.Core.ReadServices;
public interface IUserProfileReader
{
    Task<(ServiceResultCode, PersonalProfileDto?)> GetUserProfileAsync(string userName);
}
