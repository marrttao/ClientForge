namespace ClientForge.Auth;

public class RedirectUnauthorizedMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/welcome",
        "/authentication"
    };

    public RedirectUnauthorizedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Allow static files (css, js, images, etc.)
        if (path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_", StringComparison.OrdinalIgnoreCase) ||
            path.Contains('.'))
        {
            await _next(context);
            return;
        }

        // Allow pages that don't require auth
        if (AllowedPaths.Contains(path.TrimEnd('/')))
        {
            await _next(context);
            return;
        }

        // If user is not authenticated — redirect to Welcome
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            context.Response.Redirect("/Welcome");
            return;
        }

        await _next(context);
    }
}

