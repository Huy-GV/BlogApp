using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace SimpleForum.Web.Middleware;

public class ErrorPageRoutingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorPageRoutingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode < 400)
        {
            return;
        }

        switch (context.Response.StatusCode)
        {
            case 400:
                RedirectWithError(
                    context,
                    "Bad Request",
                    "Invalid request or request on resource in invalid state");
                break;

            // 401 is not handled here since it already redirects to log in page correctly
            case 403:
                RedirectWithError(
                    context,
                    "Forbidden",
                    "You do not have permission to view this page or perform the action");
                break;

            case 404:
                RedirectWithError(
                    context,
                    "Not Found",
                    "Resource not found");
                break;

            case 500:
                RedirectWithError(
                    context,
                    "Internal Server Error",
                    string.Empty);
                break;

            default:
                break;
        }
    }

    private static void RedirectWithError(HttpContext context, string errorMessage, string errorDescription)
    {
        context.Response.Redirect($"/Error/Error/?ErrorMessage={Uri.EscapeDataString(errorMessage)}&ErrorDescription={Uri.EscapeDataString(errorDescription)}");
    }
}
