using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorBlog.Communication;
using System;

namespace RazorBlog.Extensions;

public static class PageModelExtensions
{
    public static IActionResult NavigateOnResult(
        this PageModel pageModel, 
        ServiceResultCode result, 
        Func<IActionResult> onSuccess)
    {
        if (result == ServiceResultCode.Success)
        {
            return onSuccess();
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