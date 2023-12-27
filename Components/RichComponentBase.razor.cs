using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using RazorBlog.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorBlog.Components;

public class RichComponentBase : ComponentBase
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public SignInManager<ApplicationUser> SignInManager { get; set; } = null!;

    [Inject]
    public UserManager<ApplicationUser> UserManager { get; set; } = null!;

    protected ClaimsPrincipal User { get; private set; } = new();

    protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    protected bool IsSignedIn => SignInManager.IsSignedIn(User);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        User = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
    }

    protected void Challenge()
    {
        NavigationManager.NavigateTo("/Authentication/LogIn", forceLoad: true);
    }

    protected void Forbid()
    {
        var message = "You are not allowed to access the requested resource";
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage='{message}'", forceLoad: true);
    }

    protected void BadRequest()
    {
        var message = "An unknown error occurred with your request";
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage='{message}'", forceLoad: true);
    }

    protected void NotFound()
    {
        var message = "Page not found";
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage='{message}'", forceLoad: true);
    }
}
