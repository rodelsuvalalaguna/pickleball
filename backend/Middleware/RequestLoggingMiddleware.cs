using System.Diagnostics;

namespace Pickleball.Api.Middleware;

// Logs method, path, status and duration only; never bodies, query strings or headers (tokens/passwords).
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await next(ctx);
        }
        finally
        {
            logger.LogInformation("{Method} {Path} -> {Status} in {Elapsed}ms",
                ctx.Request.Method, ctx.Request.Path.Value, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
