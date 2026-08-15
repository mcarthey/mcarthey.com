using System.Text.Json;
using mcarthey.com.Honeypot;
using Microsoft.AspNetCore.DataProtection;

namespace mcarthey.com.Hack;

// Server-authoritative session state for the hack puzzle. Cookie is
// DataProtection-signed; tampered cookies fail Unprotect and get logged
// as "cookie-tamper" bait entries alongside the honeypot hits.
public sealed record SessionState(string Role, DateTimeOffset? AuthenticatedAt = null)
{
    public static readonly SessionState Guest = new("guest");
    public bool IsAuthenticated => Role == "cracker" && AuthenticatedAt is not null;
}

public sealed class GameSession
{
    public const string CookieName = "mcarthey.hack.session";
    private readonly IDataProtector _protector;
    private readonly HoneypotLogger _honeypot;

    public GameSession(IDataProtectionProvider dpp, HoneypotLogger honeypot)
    {
        _protector = dpp.CreateProtector("mcarthey.hack.session.v1");
        _honeypot = honeypot;
    }

    public SessionState Read(HttpContext ctx)
    {
        if (!ctx.Request.Cookies.TryGetValue(CookieName, out var protectedValue) || string.IsNullOrEmpty(protectedValue))
            return SessionState.Guest;

        try
        {
            var json = _protector.Unprotect(protectedValue);
            return JsonSerializer.Deserialize<SessionState>(json, HackJsonContext.Default.SessionState) ?? SessionState.Guest;
        }
        catch
        {
            // Cookie exists but signature invalid → forgery attempt.
            _honeypot.Enqueue(new HoneypotEntry(
                Timestamp: DateTimeOffset.UtcNow,
                Bait: "cookie-tamper",
                RemoteIp: ctx.Connection.RemoteIpAddress?.ToString() ?? "",
                Method: ctx.Request.Method,
                Path: ctx.Request.Path.Value ?? "",
                QueryString: null,
                UserAgent: ctx.Request.Headers.UserAgent.ToString(),
                Referer: ctx.Request.Headers.Referer.ToString(),
                Headers: new Dictionary<string, string>(),
                BodySnippet: $"cookie[0..8]={protectedValue[..Math.Min(8, protectedValue.Length)]}",
                BodyLength: protectedValue.Length));
            // Also delete the bad cookie so we don't re-log on every request.
            ctx.Response.Cookies.Delete(CookieName);
            return SessionState.Guest;
        }
    }

    public void Write(HttpContext ctx, SessionState state)
    {
        var json = JsonSerializer.Serialize(state, HackJsonContext.Default.SessionState);
        var payload = _protector.Protect(json);
        ctx.Response.Cookies.Append(CookieName, payload, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,      // browser sees HTTPS via Caddy; localhost gets a browser exemption in dev
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(30),
            Path = "/",
        });
    }

    public void Clear(HttpContext ctx) => ctx.Response.Cookies.Delete(CookieName);
}

[System.Text.Json.Serialization.JsonSerializable(typeof(SessionState))]
internal sealed partial class HackJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
