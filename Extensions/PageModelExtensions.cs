using BlogApp.Models;
using BlogApp.Pages;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApp.Extensions
{
    public static class PageModelExtensions
    {
        public static bool IsUserAuthenticated(this PageModel pageModel) => pageModel.User.Identity?.IsAuthenticated ?? false;
    }
}