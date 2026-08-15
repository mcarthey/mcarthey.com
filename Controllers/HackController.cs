using mcarthey.com.Hack;
using mcarthey.com.Honeypot;
using Microsoft.AspNetCore.Mvc;

namespace mcarthey.com.Controllers;

// Server-side game state + fake shell dispatch for the crackable-login puzzle.
// The client-side terminal.js POSTs unknown commands to /api/hack/exec; this
// controller runs them against the FakeFilesystem and returns text output.
// Successful LOGIN sets a DataProtection-signed session cookie.
[ApiController]
[Route("api/hack")]
public sealed class HackController : ControllerBase
{
    // Correct credentials for the puzzle. Case-insensitive by design --
    // making it case-sensitive would frustrate keyboard-typing humans
    // without stopping bots (bots don't play the puzzle anyway).
    private const string CorrectUser = "falken";
    private const string CorrectPass = "joshua";

    private readonly GameSession _session;
    private readonly HoneypotLogger _honeypot;

    public HackController(GameSession session, HoneypotLogger honeypot)
    {
        _session = session;
        _honeypot = honeypot;
    }

    // POST /api/hack/exec { command: "cat /etc/motd" }
    [HttpPost("exec")]
    public IActionResult Exec([FromBody] ExecRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Command))
            return Ok(new ExecResponse(new[] { "" }));

        var state = _session.Read(HttpContext);
        var parts = req.Command.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();
        var args = parts.Length > 1 ? parts[1] : "";

        var output = cmd switch
        {
            "ls" or "dir" => RunLs(args),
            "cat" or "type" or "more" => RunCat(args),
            "whoami" => RunWhoami(state),
            "history" => RunHistory(),
            "logout" or "exit" => RunLogout(state),
            "wargames" => RunWargames(state),
            "pwd" => new[] { "/" },
            "id" => RunWhoami(state),  // alias
            _ => new[] { $"{cmd}: command not found" }
        };

        return Ok(new ExecResponse(output));
    }

    // POST /api/hack/login { username: "falken", password: "joshua" }
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var username = (req.Username ?? "").Trim();
        var password = req.Password ?? "";
        var success = string.Equals(username, CorrectUser, StringComparison.OrdinalIgnoreCase)
                      && string.Equals(password, CorrectPass, StringComparison.OrdinalIgnoreCase);

        // Every attempt goes into the honeypot log -- successful cracks
        // show on the wall as celebrations, failures as brute-force attempts.
        _honeypot.Enqueue(new HoneypotEntry(
            Timestamp: DateTimeOffset.UtcNow,
            Bait: success ? "crack-success" : "crack-attempt",
            RemoteIp: ip,
            Method: "POST",
            Path: "/api/hack/login",
            QueryString: null,
            UserAgent: HttpContext.Request.Headers.UserAgent.ToString(),
            Referer: HttpContext.Request.Headers.Referer.ToString(),
            Headers: new Dictionary<string, string>(),
            BodySnippet: $"username={username}&password={password}",
            BodyLength: username.Length + password.Length + 20));

        if (success)
        {
            _session.Write(HttpContext, new SessionState("cracker", DateTimeOffset.UtcNow));
            return Ok(new ExecResponse(new[]
            {
                "",
                "> ACCESS GRANTED. WELCOME PROFESSOR FALKEN.",
                "",
                "  You have unlocked WOPR mode.",
                "  Your breach has been recorded on the wall of shame.",
                "",
                "  Try: WARGAMES, WHOAMI, LOGOUT",
                "",
            }));
        }

        return Ok(new ExecResponse(new[]
        {
            "",
            "> ACCESS DENIED",
            "",
            "  Invalid credentials. Attempt logged.",
            "",
        }));
    }

    // GET /api/hack/session -- client can query current auth state (unused for now)
    [HttpGet("session")]
    public IActionResult Session()
    {
        var state = _session.Read(HttpContext);
        return Ok(new { role = state.Role, authenticatedAt = state.AuthenticatedAt });
    }

    // ---------- command handlers ----------

    private static string[] RunLs(string args)
    {
        var path = NormalizePath(string.IsNullOrEmpty(args) ? "/" : args);
        if (!FakeFilesystem.Directories.TryGetValue(path, out var entries))
            return new[] { $"ls: cannot access '{args}': No such file or directory" };

        var lines = new List<string> { $" Directory of {path}", "" };
        foreach (var e in entries)
        {
            var mode = e.IsDir ? "drwxr-xr-x" : "-rw-r--r--";
            var sizeStr = e.IsDir ? "4096" : e.Size.ToString();
            var name = e.IsDir ? $"{e.Name}/" : e.Name;
            lines.Add($"{mode}  root  {sizeStr,8}  {name}");
        }
        return lines.ToArray();
    }

    private static string[] RunCat(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
            return new[] { "cat: missing operand. usage: cat <file>" };
        var path = NormalizePath(args);
        if (!FakeFilesystem.Files.TryGetValue(path, out var content))
            return new[] { $"cat: {args}: No such file or directory" };
        return content.Split('\n');
    }

    private static string[] RunWhoami(SessionState state)
    {
        if (state.IsAuthenticated)
            return new[] { $"falken  (authenticated at {state.AuthenticatedAt:yyyy-MM-dd HH:mm} UTC)" };
        return new[] { "guest" };
    }

    private static string[] RunHistory() => new[]
    {
        "  1  ls /home",
        "  2  cd /home/falken",
        "  3  cat README.old",
        "  4  vi login.c",
        "  5  make",
        "  6  ./login",
        "  7  # backdoor still works, note to self: NEVER PATCH",
        "  8  logout",
    };

    private string[] RunLogout(SessionState state)
    {
        _session.Clear(HttpContext);
        return new[] { state.IsAuthenticated ? "Logged out." : "(not logged in)" };
    }

    private static string[] RunWargames(SessionState state)
    {
        if (!state.IsAuthenticated)
            return new[] { "wargames: permission denied. Try LOGIN first." };
        return new[]
        {
            "",
            "  GREETINGS PROFESSOR FALKEN.",
            "  SHALL WE PLAY A GAME?",
            "",
            "  Available games:",
            "    * Tic-Tac-Toe",
            "    * Chess",
            "    * Global Thermonuclear War",
            "",
            "  (game engine coming soon -- for now, the winning move is not to play.)",
            "",
        };
    }

    // Very simple path normalization. Rejects .. traversal (no way to escape
    // the fake filesystem anyway since it's a dictionary lookup, but keeping
    // it explicit prevents "/foo/../bar" collision confusion). Always leaves
    // paths as absolute, lowercase, no trailing slash except for root.
    private static string NormalizePath(string p)
    {
        p = p.Trim();
        if (p == "" || p == ".") return "/";
        if (!p.StartsWith('/')) p = "/" + p;
        if (p.Length > 1 && p.EndsWith('/')) p = p[..^1];
        // Collapse any .. or . segments to their most obvious meaning; the
        // fake-filesystem lookup will just miss on anything weird.
        var segments = p.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var stack = new List<string>();
        foreach (var seg in segments)
        {
            if (seg == "..") { if (stack.Count > 0) stack.RemoveAt(stack.Count - 1); }
            else if (seg == ".") { }
            else stack.Add(seg);
        }
        return "/" + string.Join('/', stack);
    }
}

public sealed record ExecRequest(string Command);
public sealed record ExecResponse(string[] Output);
public sealed record LoginRequest(string? Username, string? Password);
