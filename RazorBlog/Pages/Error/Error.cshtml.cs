using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorBlog.Pages.Error;

public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet =true)]
    public string ErrorMessage { get; private set; } = "An unknown error occurred";

    [BindProperty(SupportsGet = true)]
    public string ErrorDescription { get; private set; } = string.Empty;

    public void OnGet(string errorMessage, string? errorDescription)
    {
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            ErrorMessage = errorMessage;
        }

        if (!string.IsNullOrWhiteSpace(errorDescription))
        {
            ErrorDescription = errorDescription;
        }
    }
}
