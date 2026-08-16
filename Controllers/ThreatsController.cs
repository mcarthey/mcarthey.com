using mcarthey.com.Honeypot;
using Microsoft.AspNetCore.Mvc;

namespace mcarthey.com.Controllers;

// Public JSON feed of top attacker IPs observed by our honeypot.
// Other honeypot operators / researchers can consume this for their own
// blocklists. Uses the same 30-day window as /Shame, filters to IPs with
// 2+ unique bait families (drops single drive-by noise).
[ApiController]
[Route("threats.json")]
public sealed class ThreatsController : ControllerBase
{
    private readonly HoneypotStats _stats;
    public ThreatsController(HoneypotStats stats) => _stats = stats;

    [HttpGet]
    [ResponseCache(Duration = 300)]  // 5-min public cache
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var data = await _stats.GetAsync(ct);
        var threats = data.SpeciesCatalog
            .Where(s => s.UniqueBaits >= 2)
            .Where(s => !s.Species.StartsWith("🏆") && !s.Species.StartsWith("🕹"))  // exclude celebrations
            .Select(s => new
            {
                ip = s.Ip,
                species = StripEmoji(s.Species),
                hits = s.Hits,
                uniqueBaits = s.UniqueBaits,
                lastSeen = s.LastSeen.ToString("o"),
            })
            .ToList();

        return Ok(new
        {
            source = "https://mcarthey.com/",
            generated = data.GeneratedAt.ToString("o"),
            windowDays = data.WindowDays,
            description = "Attacker IPs observed by the mcarthey.com honeypot. Free to consume; no warranty. See /Shame for the human-readable view.",
            count = threats.Count,
            threats,
        });
    }

    private static string StripEmoji(string s)
    {
        var i = 0;
        while (i < s.Length && (char.IsSurrogate(s[i]) || s[i] > 127)) i++;
        return s[i..].TrimStart();
    }
}
