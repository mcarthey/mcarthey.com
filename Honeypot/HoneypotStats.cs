using System.Text.Json;

namespace mcarthey.com.Honeypot;

// Reads the last N days of honeypot JSONL logs, computes aggregations,
// caches the result for 60s. Not a database -- just an in-memory reducer
// over the same log files the middleware writes to.
public sealed class HoneypotStats
{
    private const int WindowDays = 30;
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(60);

    private readonly string _logDir;
    private readonly ILogger<HoneypotStats> _log;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private WallOfShameData? _cached;
    private DateTimeOffset _cachedAt = DateTimeOffset.MinValue;

    public HoneypotStats(IConfiguration config, ILogger<HoneypotStats> log)
    {
        _logDir = config["Honeypot:LogDirectory"] ?? "logs/honeypot";
        _log = log;
    }

    public async Task<WallOfShameData> GetAsync(CancellationToken ct = default)
    {
        if (_cached is not null && DateTimeOffset.UtcNow - _cachedAt < CacheLifetime)
            return _cached;

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_cached is not null && DateTimeOffset.UtcNow - _cachedAt < CacheLifetime)
                return _cached;

            _cached = await ComputeAsync(ct).ConfigureAwait(false);
            _cachedAt = DateTimeOffset.UtcNow;
            return _cached;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<WallOfShameData> ComputeAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_logDir))
            return WallOfShameData.Empty;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-WindowDays);
        var last24h = DateTimeOffset.UtcNow.AddHours(-24);

        var totalHits = 0;
        var hits24h = 0;
        var baitCounts = new Dictionary<string, int>();
        var ipCounts = new Dictionary<string, IpAgg>();
        var uaCounts = new Dictionary<string, int>();
        var recentHits = new List<HitSummary>();
        var funnyPosts = new List<HitSummary>();
        var successfulCracks = new List<HitSummary>();

        // Per-IP bait sets for taxonomy classification.
        var perIpBaits = new Dictionary<string, HashSet<string>>();
        var perIpTotal = new Dictionary<string, int>();

        var files = Directory.EnumerateFiles(_logDir, "*.jsonl")
            .Select(p => (Path: p, Name: Path.GetFileNameWithoutExtension(p)))
            .Where(f => DateTime.TryParse(f.Name, out var d) && d >= cutoff.UtcDateTime.Date)
            .OrderBy(f => f.Name);

        foreach (var (path, _) in files)
        {
            ct.ThrowIfCancellationRequested();
            await foreach (var line in File.ReadLinesAsync(path, ct).ConfigureAwait(false))
            {
                HoneypotEntry? entry;
                try { entry = JsonSerializer.Deserialize(line, HoneypotJsonContext.Default.HoneypotEntry); }
                catch { continue; }
                if (entry is null) continue;

                totalHits++;
                if (entry.Timestamp >= last24h) hits24h++;

                baitCounts[entry.Bait] = baitCounts.GetValueOrDefault(entry.Bait) + 1;

                if (!string.IsNullOrEmpty(entry.RemoteIp))
                {
                    if (ipCounts.TryGetValue(entry.RemoteIp, out var agg))
                        ipCounts[entry.RemoteIp] = agg with { Count = agg.Count + 1, LastSeen = entry.Timestamp };
                    else
                        ipCounts[entry.RemoteIp] = new IpAgg(entry.RemoteIp, 1, entry.Timestamp);

                    if (!perIpBaits.TryGetValue(entry.RemoteIp, out var set))
                        perIpBaits[entry.RemoteIp] = set = new HashSet<string>();
                    set.Add(entry.Bait);
                    perIpTotal[entry.RemoteIp] = perIpTotal.GetValueOrDefault(entry.RemoteIp) + 1;
                }

                if (!string.IsNullOrEmpty(entry.UserAgent))
                    uaCounts[entry.UserAgent] = uaCounts.GetValueOrDefault(entry.UserAgent) + 1;

                if (recentHits.Count < 20 ||
                    entry.Timestamp > recentHits[^1].Timestamp)
                {
                    recentHits.Add(new HitSummary(entry.Timestamp, entry.Bait, entry.RemoteIp, entry.Method, entry.Path, RedactCreds(entry.BodySnippet)));
                }

                // Successful cracks are celebrations, not brute-force noise.
                // Log them separately and never expose the winning password
                // (would spoil the puzzle for the next visitor).
                if (entry.Bait == "crack-success")
                {
                    successfulCracks.Add(new HitSummary(entry.Timestamp, entry.Bait, entry.RemoteIp, entry.Method, entry.Path, entry.UserAgent));
                }
                else if (entry.BodyLength > 0 && !string.IsNullOrEmpty(entry.BodySnippet))
                {
                    funnyPosts.Add(new HitSummary(entry.Timestamp, entry.Bait, entry.RemoteIp, entry.Method, entry.Path, RedactCreds(entry.BodySnippet)));
                }
            }
        }

        return new WallOfShameData(
            TotalHits: totalHits,
            Hits24h: hits24h,
            WindowDays: WindowDays,
            GeneratedAt: DateTimeOffset.UtcNow,
            TopBaits: baitCounts.OrderByDescending(kv => kv.Value).Take(10)
                .Select(kv => new BaitAgg(kv.Key, kv.Value)).ToList(),
            TopIps: ipCounts.Values.OrderByDescending(x => x.Count).Take(15).ToList(),
            TopUserAgents: uaCounts.OrderByDescending(kv => kv.Value).Take(10)
                .Select(kv => new UserAgentAgg(kv.Key, kv.Value)).ToList(),
            RecentHits: recentHits.OrderByDescending(h => h.Timestamp).Take(25).ToList(),
            FunnyPosts: funnyPosts.OrderByDescending(h => h.Timestamp).Take(15).ToList(),
            SuccessfulCracks: successfulCracks.OrderByDescending(h => h.Timestamp).Take(20).ToList(),
            SpeciesCatalog: BuildSpeciesCatalog(perIpBaits, perIpTotal, ipCounts));
    }

    // Classifies each IP into a species based on which bait categories they hit.
    // Rules are priority-ordered; first match wins. Rare/interesting species
    // sort to the top so they're visible on the shame page.
    private static List<SpeciesEntry> BuildSpeciesCatalog(
        Dictionary<string, HashSet<string>> perIpBaits,
        Dictionary<string, int> perIpTotal,
        Dictionary<string, IpAgg> ipCounts)
    {
        var list = new List<SpeciesEntry>();
        foreach (var (ip, baits) in perIpBaits)
        {
            var species = Classify(baits);
            list.Add(new SpeciesEntry(
                Ip: ip,
                Species: species,
                Hits: perIpTotal[ip],
                UniqueBaits: baits.Count,
                LastSeen: ipCounts.TryGetValue(ip, out var agg) ? agg.LastSeen : DateTimeOffset.MinValue,
                SpeciesRank: SpeciesRank(species)));
        }
        // Interesting species (low rank number) first, then by hits.
        return list
            .OrderBy(e => e.SpeciesRank)
            .ThenByDescending(e => e.Hits)
            .Take(30)
            .ToList();
    }

    private static string Classify(HashSet<string> baits)
    {
        bool Has(string b) => baits.Contains(b);
        bool HasAny(params string[] bs) => bs.Any(b => baits.Contains(b));
        int Count(params string[] bs) => bs.Count(b => baits.Contains(b));

        if (Has("crack-success")) return "🏆 Cracker (welcomed)";
        if (Has("konami")) return "🕹 Konami-code enterer";
        if (Has("llm-caught")) return "🤖 LLM scanner";
        if (Count("vault", "backup-dir", "secret", "internal-admin") >= 2) return "🕵 Robots-inverse crawler";
        if (Has("dotenv") && Has("api-internal")) return "🧠 Env-parser (Tier 2)";
        if (HasAny("wp-plugins", "wp-users", "wp-backup") && HasAny("wp-login", "wp-setup"))
            return "🕸 WP spider (HTML-parsing)";
        if (HasAny("phpma-export", "phpma-import") && Has("phpmyadmin"))
            return "🕸 phpMyAdmin spider";
        if (Has("cookie-tamper")) return "🔓 Cookie-forger";
        if (Has("meta-cheat")) return "🎯 Meta-cheater";
        if (Has("crack-attempt")) return "🔨 Brute-forcer";
        if (baits.Count >= 5) return "🎣 Multi-tool scanner";
        if (baits.Count == 1)
        {
            var only = baits.First();
            if (only == "wp-login") return "👟 WP-only drive-by";
            if (only == "dotenv") return "👟 .env grabber";
            if (only.StartsWith("git")) return "👟 git leak scanner";
            if (only == "restricted") return "🚪 Old-URL follower";
            return "👟 Drive-by scanner";
        }
        return "🔍 Explorer";
    }

    // Lower = more interesting = sorted higher on the wall.
    private static int SpeciesRank(string species) => species switch
    {
        "🏆 Cracker (welcomed)" => 0,
        "🕹 Konami-code enterer" => 1,
        "🤖 LLM scanner" => 2,
        "🕵 Robots-inverse crawler" => 3,
        "🧠 Env-parser (Tier 2)" => 4,
        "🕸 WP spider (HTML-parsing)" => 5,
        "🕸 phpMyAdmin spider" => 6,
        "🔓 Cookie-forger" => 7,
        "🎯 Meta-cheater" => 8,
        "🔨 Brute-forcer" => 9,
        "🎣 Multi-tool scanner" => 10,
        "🔍 Explorer" => 11,
        _ => 12,  // drive-bys
    };

    // Redact credential-shaped values in form-urlencoded POST bodies.
    // Keeps username visible (juice for the wall), redacts password-shaped fields.
    private static string? RedactCreds(string? snippet)
    {
        if (string.IsNullOrEmpty(snippet)) return snippet;
        var parts = snippet.Split('&');
        for (var i = 0; i < parts.Length; i++)
        {
            var eq = parts[i].IndexOf('=');
            if (eq < 0) continue;
            var key = parts[i][..eq].ToLowerInvariant();
            if (key.Contains("pass") || key.Contains("pwd") || key.Contains("secret") || key.Contains("token") || key.Contains("key"))
            {
                var value = parts[i][(eq + 1)..];
                var keep = Math.Min(2, value.Length);
                parts[i] = $"{parts[i][..eq]}={value[..keep]}{new string('*', Math.Max(0, value.Length - keep))}";
            }
        }
        return string.Join('&', parts);
    }
}

public sealed record WallOfShameData(
    int TotalHits,
    int Hits24h,
    int WindowDays,
    DateTimeOffset GeneratedAt,
    List<BaitAgg> TopBaits,
    List<IpAgg> TopIps,
    List<UserAgentAgg> TopUserAgents,
    List<HitSummary> RecentHits,
    List<HitSummary> FunnyPosts,
    List<HitSummary> SuccessfulCracks,
    List<SpeciesEntry> SpeciesCatalog)
{
    public static readonly WallOfShameData Empty = new(0, 0, 30, DateTimeOffset.UtcNow,
        new(), new(), new(), new(), new(), new(), new());
}

public sealed record BaitAgg(string Bait, int Count);
public sealed record IpAgg(string Ip, int Count, DateTimeOffset LastSeen);
public sealed record UserAgentAgg(string UserAgent, int Count);
public sealed record HitSummary(DateTimeOffset Timestamp, string Bait, string RemoteIp, string Method, string Path, string? BodySnippet);
public sealed record SpeciesEntry(string Ip, string Species, int Hits, int UniqueBaits, DateTimeOffset LastSeen, int SpeciesRank);
