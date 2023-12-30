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

    public void NavigateToChallenge()
    {
        NavigationManager.NavigateTo("/Authentication/LogIn", forceLoad: true);
    }

    public void NavigateToForbid(string? message = null, string? description = null)
    {
        message ??= "You are not allowed to access the requested resource";
        description ??= string.Empty;
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage={message}&ErrorDescription={description}", forceLoad: true);
    }

    public void NavigateToBadRequest(string? message = null, string? description = null)
    {
        message ??= "An unknown error occurred with your request";
        description ??= string.Empty;
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage={message}&ErrorDescription={description}", forceLoad: true);
    }

    public void NavigateToNotFound(string? message = null, string? description = null)
    {
        message ??= "Page not found";
        description ??= string.Empty;
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage={message}&ErrorDescription={description}", forceLoad: true);
    }
}
