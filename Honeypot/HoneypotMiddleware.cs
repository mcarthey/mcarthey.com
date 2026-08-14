using System.Text;

namespace mcarthey.com.Honeypot;

// Intercepts requests to known bot-target paths BEFORE ASP.NET routing.
// If the path matches a bait, log the hit and serve the fake response.
// Otherwise pass through to the rest of the pipeline.
//
// Deliberately does NOT echo any request input into the response body —
// scanners sometimes probe for reflected XSS, and reflecting a payload
// would turn the honeypot into a real XSS hole.
public sealed class HoneypotMiddleware
{
    private const int MaxBodySnippet = 512;
    private readonly RequestDelegate _next;
    private readonly HoneypotLogger _logger;

    public HoneypotMiddleware(RequestDelegate next, HoneypotLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value;
        if (path is null || !HoneypotBaits.Table.TryGetValue(path, out var bait))
        {
            await _next(ctx).ConfigureAwait(false);
            return;
        }

        // Read POST body up to snippet cap so we log attempted credentials /
        // command injection payloads. Doesn't reflect any of it into the response.
        string? snippet = null;
        int bodyLen = 0;
        if (HttpMethods.IsPost(ctx.Request.Method) || HttpMethods.IsPut(ctx.Request.Method))
        {
            ctx.Request.EnableBuffering();
            using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            bodyLen = body.Length;
            snippet = body.Length > MaxBodySnippet ? body[..MaxBodySnippet] : body;
            ctx.Request.Body.Position = 0;
        }

        var entry = new HoneypotEntry(
            Timestamp: DateTimeOffset.UtcNow,
            Bait: bait.Name,
            RemoteIp: ctx.Connection.RemoteIpAddress?.ToString() ?? "",
            Method: ctx.Request.Method,
            Path: path,
            QueryString: ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : null,
            UserAgent: ctx.Request.Headers.UserAgent.ToString(),
            Referer: ctx.Request.Headers.Referer.ToString(),
            Headers: ctx.Request.Headers
                .Where(h => !h.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase),
            BodySnippet: snippet,
            BodyLength: bodyLen);
        _logger.Enqueue(entry);

        ctx.Response.StatusCode = bait.StatusCode;
        ctx.Response.ContentType = bait.ContentType;
        // Look mundane. Real WordPress / phpMyAdmin sit behind boring servers.
        ctx.Response.Headers["Server"] = "nginx/1.18.0 (Ubuntu)";
        await ctx.Response.WriteAsync(bait.Body, Encoding.UTF8).ConfigureAwait(false);
    }
}
