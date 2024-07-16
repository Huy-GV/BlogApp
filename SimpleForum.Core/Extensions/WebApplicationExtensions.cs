using Microsoft.AspNetCore.Builder;

namespace SimpleForum.Core.Extensions;
public static class WebApplicationExtensions
{
    public static void UseMigrationsEndpointExtension(this WebApplication app)
    {
        app.UseMigrationsEndPoint();
    }
}
