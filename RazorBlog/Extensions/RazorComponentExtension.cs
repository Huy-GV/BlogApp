using System;
using RazorBlog.Core.Communication;
using RazorBlog.Components;

namespace RazorBlog.Extensions;

public static class RazorComponentExtension
{
    /// <summary>
    /// Navigate to another page based on a failure result.
    /// The navigation will not occur until the calling function returns.
    /// </summary>
    /// <param name="component">Current Razor component.</param>
    /// <param name="resultCode">Failing service result code.</param>
    /// <exception cref="ArgumentException">Thrown when the service result is success.</exception>
    public static void NavigateOnError(this RichComponentBase component, ServiceResultCode resultCode)
    {
        switch (resultCode)
        {
            case ServiceResultCode.Success:
                throw new ArgumentException("Service result is success");
            case ServiceResultCode.Error:
                component.NavigateToBadRequest();
                break;
            case ServiceResultCode.NotFound:
                component.NavigateToNotFound();
                break;
            case ServiceResultCode.Unauthenticated:
                component.NavigateToChallenge();
                break;
            case ServiceResultCode.Unauthorized:
                component.NavigateToForbid();
                break;
            case ServiceResultCode.InvalidState:
            case ServiceResultCode.InvalidArguments:
            default: 
                component.NavigateToBadRequest();
                break;
        }
    }
}
