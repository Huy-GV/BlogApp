using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorBlog.Core.Communication;
using System;

namespace RazorBlog.Web.Extensions;

public static class PageModelExtensions
{
    /// <summary>
    /// Returns a navigation result based on the service result code.
    /// </summary>
    /// <param name="pageModel">Current Razor Page.</param>
    /// <param name="result">Service result code.</param>
    /// <param name="onSuccess">Function called when the service result is success.</param>
    /// <returns>Result of <paramref name="onSuccess"/> if service result is success; otherwise, a navigation result.</returns>
    public static IActionResult NavigateOnResult(
        this PageModel pageModel,
        ServiceResultCode result,
        Func<IActionResult> onSuccess)
    {
        return result == ServiceResultCode.Success ? onSuccess() : pageModel.NavigateOnError(result);
    }

    /// <summary>
    /// Returns a navigation result based on a failure service result code.
    /// </summary>
    /// <param name="pageModel">Current Razor Page.</param>
    /// <param name="result">Failing service result code.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="result"/> is success.</exception>
    /// <returns>Navigation result to an error page.</returns>
    public static IActionResult NavigateOnError(
        this PageModel pageModel,
        ServiceResultCode result)
    {
        if (result == ServiceResultCode.Success)
        {
            throw new ArgumentException("Service result is success");
        }

        return result switch
        {
            ServiceResultCode.NotFound => pageModel.NotFound(),
            ServiceResultCode.Unauthorized => pageModel.StatusCode(403),
            ServiceResultCode.Unauthenticated => pageModel.Challenge(),
            ServiceResultCode.InvalidArguments or
                ServiceResultCode.InvalidState or
                ServiceResultCode.Error => pageModel.BadRequest(),
            _ => pageModel.BadRequest()
        };
    }
}