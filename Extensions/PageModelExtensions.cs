using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorBlog.Extensions;

public static class PageModelExtensions
{
    public static bool IsUserAuthenticated(this PageModel pageModel)
    {
        return pageModel.User.Identity?.IsAuthenticated ?? false;
    }
}