using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using RazorBlog.Models;
using System;
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
    public UserManager<ApplicationUser> UserManager { get; set; } = null!;

    protected ClaimsPrincipal CurrentUser { get; private set; } = new();

    protected string CurrentUserName => CurrentUser.Identity?.Name ?? string.Empty;

    protected bool IsAuthenticated => CurrentUser?.Identity?.IsAuthenticated ?? false;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        CurrentUser = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
    }

    public void NavigateToChallenge()
    {
        NavigationManager.NavigateTo("/Authentication/LogIn", forceLoad: true);
    }

    public void NavigateToForbid(string? message = null, string? description = null)
    {
        message ??= "You are not allowed to access the requested resource";
        description ??= string.Empty;
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage={Uri.EscapeDataString(message)}&ErrorDescription={Uri.EscapeDataString(description)}", forceLoad: true);
    }

    public void NavigateToBadRequest(string? message = null, string? description = null)
    {
        message ??= "An unknown error occurred with your request";
        description ??= string.Empty;
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage={Uri.EscapeDataString(message)}&ErrorDescription={Uri.EscapeDataString(description)}", forceLoad: true);
    }

    public void NavigateToNotFound(string? message = null, string? description = null)
    {
        message ??= "Page not found";
        description ??= string.Empty;
        NavigationManager.NavigateTo($"/Error/Error?ErrorMessage={Uri.EscapeDataString(message)}&ErrorDescription={Uri.EscapeDataString(description)}", forceLoad: true);
    }
}
