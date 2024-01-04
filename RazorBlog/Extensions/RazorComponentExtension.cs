using RazorBlog.Communication;
using RazorBlog.Components;

namespace RazorBlog.Extensions;

public static class RazorComponentExtension
{
    public static void NavigateOnError(this RichComponentBase component, ServiceResultCode resultCode)
    {
        switch (resultCode)
        {
            case ServiceResultCode.Success:
                break;
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
