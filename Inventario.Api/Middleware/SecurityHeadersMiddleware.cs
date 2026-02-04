namespace Inventario.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {           
        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Enable XSS filter in browsers
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Strict Transport Security (HSTS)
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // Referrer Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content Security Policy - Más permisivo para Swagger
        var isSwaggerPath = context.Request.Path.StartsWithSegments("/swagger");
        
        if (isSwaggerPath)
        {
            // CSP permisivo para Swagger (solo en desarrollo)
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self' 'unsafe-inline' 'unsafe-eval' data: blob:; " +
                "style-src 'self' 'unsafe-inline' fonts.googleapis.com; " +
                "font-src 'self' fonts.gstatic.com; " +
                "img-src 'self' data: blob:; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval';");
        }
        else
        {
            // CSP restrictivo para API endpoints
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "frame-ancestors 'none'; " +
                "object-src 'none'; " +
                "base-uri 'self';");
        }

        // Permissions Policy (formerly Feature-Policy)
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        // Cache control for sensitive data
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
