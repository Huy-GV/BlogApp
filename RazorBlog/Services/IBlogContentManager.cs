using RazorBlog.Communication;
using RazorBlog.Data.ViewModels;
using System.Threading.Tasks;

namespace RazorBlog.Services;
public interface IBlogContentManager
{
    Task<(ServiceResultCode, int)> CreateBlogAsync(CreateBlogViewModel createBlogViewModel, string userName);
    Task<ServiceResultCode> DeleteBlogAsync(int blogId, string userName);
    Task<ServiceResultCode> UpdateBlog(EditBlogViewModel editBlogViewModel, string userName);
}