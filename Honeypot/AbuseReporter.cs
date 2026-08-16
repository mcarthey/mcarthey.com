using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mcarthey.com.Honeypot;

// Background service that periodically scans the honeypot log, identifies
// high-confidence attacker IPs, and reports them to AbuseIPDB via their v2
// API. Rate-limited to once per IP per 24h to respect their fair-use terms
// and to avoid spamming the community feed.
//
// Gracefully no-ops if AbuseIPDB:ApiKey is missing -- the site still works
// without a key, we just don't publish reports.
public sealed class AbuseReporter : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MinReportGap = TimeSpan.FromHours(24);
    private static readonly TimeSpan NormalWindow = TimeSpan.FromHours(24);
    private static readonly TimeSpan BackfillWindow = TimeSpan.FromDays(30);
    private const int MinUniqueBaitsToReport = 2;

    private readonly IConfiguration _config;
    private readonly ILogger<AbuseReporter> _log;
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _logDir;
    private readonly string _stateFile;

    public AbuseReporter(IConfiguration config, ILogger<AbuseReporter> log, IHttpClientFactory httpFactory)
    {
        _config = config;
        _log = log;
        _httpFactory = httpFactory;
        _logDir = config["Honeypot:LogDirectory"] ?? "logs/honeypot";
        _stateFile = Path.Combine(_logDir, "abuse-reported.json");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiKey = _config["AbuseIPDB:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _log.LogInformation("AbuseReporter disabled: AbuseIPDB:ApiKey not configured.");
            return;
        }

        _log.LogInformation("AbuseReporter enabled. Scan interval: {Interval}", ScanInterval);
        // First scan waits a short delay so we don't fire during app startup.
        try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); }
        catch (OperationCanceledException) { return; }

        // Every scan: if the state file is missing (never reported, or
        // manually deleted to force a backfill), scan the last 30 days
        // instead of the normal 24h. Historical IPs still have community
        // value; deleting abuse-reported.json is the "force backfill" knob.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var doBackfill = !File.Exists(_stateFile);
                var window = doBackfill ? BackfillWindow : NormalWindow;
                if (doBackfill)
                {
                    _log.LogInformation("AbuseReporter state file absent; backfilling last {Days} days.", BackfillWindow.TotalDays);
                }
                await ScanAndReportAsync(apiKey, window, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { _log.LogError(ex, "AbuseReporter scan failed"); }

            try { await Task.Delay(ScanInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task ScanAndReportAsync(string apiKey, TimeSpan window, CancellationToken ct)
    {
        if (!Directory.Exists(_logDir)) return;

        var state = await LoadStateAsync(ct);
        var cutoff = DateTimeOffset.UtcNow - window;
        var perIp = new Dictionary<string, IpAggregate>();

        // Backfill needs the whole 30-day file set; normal scan only needs
        // the last 3 days of files to cover the 24h cutoff.
        var fileTake = window > NormalWindow ? 31 : 3;
        var files = Directory.EnumerateFiles(_logDir, "*.jsonl")
            .OrderByDescending(f => f)
            .Take(fileTake);

        foreach (var file in files)
        {
            await foreach (var line in File.ReadLinesAsync(file, ct))
            {
                HoneypotEntry? entry;
                try { entry = JsonSerializer.Deserialize(line, HoneypotJsonContext.Default.HoneypotEntry); }
                catch { continue; }
                if (entry is null || entry.Timestamp < cutoff) continue;
                if (string.IsNullOrEmpty(entry.RemoteIp)) continue;
                if (IsPrivateOrLoopback(entry.RemoteIp)) continue;

                if (!perIp.TryGetValue(entry.RemoteIp, out var agg))
                    perIp[entry.RemoteIp] = agg = new IpAggregate();
                agg.Baits.Add(entry.Bait);
                agg.HitCount++;
                if (entry.Timestamp > agg.LastSeen) agg.LastSeen = entry.Timestamp;
            }
        }

        var http = _httpFactory.CreateClient();
        http.BaseAddress = new Uri("https://api.abuseipdb.com/api/v2/");
        http.DefaultRequestHeaders.Add("Key", apiKey);
        http.DefaultRequestHeaders.Add("Accept", "application/json");

        var reportedThisScan = 0;
        foreach (var (ip, agg) in perIp)
        {
            ct.ThrowIfCancellationRequested();
            if (agg.Baits.Count < MinUniqueBaitsToReport) continue;
            if (state.LastReported.TryGetValue(ip, out var last) && DateTimeOffset.UtcNow - last < MinReportGap) continue;

            var (categories, comment) = ClassifyForReport(agg);
            var reported = await SubmitReportAsync(http, ip, categories, comment, agg.LastSeen, ct);
            if (reported)
            {
                state.LastReported[ip] = DateTimeOffset.UtcNow;
                reportedThisScan++;
                // Be polite to their API even though we're well under rate limit.
                try { await Task.Delay(TimeSpan.FromSeconds(1), ct); }
                catch (OperationCanceledException) { break; }
            }
        }

        if (reportedThisScan > 0)
        {
            await SaveStateAsync(state, ct);
            _log.LogInformation("AbuseReporter submitted {Count} report(s)", reportedThisScan);
        }
    }

    private async Task<bool> SubmitReportAsync(HttpClient http, string ip, string categories, string comment, DateTimeOffset lastSeen, CancellationToken ct)
    {
        try
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("ip", ip),
                new KeyValuePair<string, string>("categories", categories),
                new KeyValuePair<string, string>("comment", comment),
                new KeyValuePair<string, string>("timestamp", lastSeen.ToString("o")),
            });
            var resp = await http.PostAsync("report", form, ct);
            if (resp.IsSuccessStatusCode) return true;

            var body = await resp.Content.ReadAsStringAsync(ct);
            _log.LogWarning("AbuseIPDB report for {Ip} rejected: HTTP {Status} -- {Body}", ip, (int)resp.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "AbuseIPDB report POST failed for {Ip}", ip);
            return false;
        }
    }

    // AbuseIPDB category codes: https://www.abuseipdb.com/categories
    // Categories are additive; multiple can apply. Default to 19 (Bad Web Bot)
    // + 21 (Web App Attack); add 18 (Brute-Force) or 15 (Hacking) if the
    // signature matches.
    private static (string categories, string comment) ClassifyForReport(IpAggregate agg)
    {
        var cats = new HashSet<int> { 19, 21 };
        if (agg.Baits.Contains("crack-attempt")) cats.Add(18);
        if (agg.Baits.Contains("meta-cheat") || agg.Baits.Contains("cookie-tamper")) cats.Add(15);
        if (agg.Baits.Contains("wp-login") || agg.Baits.Contains("phpmyadmin")) cats.Add(21);

        var baitList = string.Join(", ", agg.Baits.OrderBy(b => b));
        var comment = $"Automated honeypot report from mcarthey.com. {agg.HitCount} hits across {agg.Baits.Count} bait families ({baitList}) in the last 24h. Full log: https://mcarthey.com/Shame";
        // AbuseIPDB comment limit is 1024 chars.
        if (comment.Length > 1000) comment = comment[..1000];
        return (string.Join(',', cats.OrderBy(c => c)), comment);
    }

    private static bool IsPrivateOrLoopback(string ip)
    {
        if (!System.Net.IPAddress.TryParse(ip, out var addr)) return true;
        if (System.Net.IPAddress.IsLoopback(addr)) return true;
        var bytes = addr.GetAddressBytes();
        if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            if (bytes[0] == 10) return true;
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
            if (bytes[0] == 192 && bytes[1] == 168) return true;
            if (bytes[0] == 169 && bytes[1] == 254) return true;
        }
        // Skip IPv6 link-local / unique-local
        if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            if (bytes[0] == 0xfe && (bytes[1] & 0xC0) == 0x80) return true;
            if (bytes[0] == 0xfc || bytes[0] == 0xfd) return true;
        }
        return false;
    }

    private async Task<ReportState> LoadStateAsync(CancellationToken ct)
    {
        if (!File.Exists(_stateFile)) return new ReportState();
        try
        {
            var json = await File.ReadAllTextAsync(_stateFile, ct);
            return JsonSerializer.Deserialize(json, AbuseJsonContext.Default.ReportState) ?? new ReportState();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "AbuseReporter state file unreadable, starting fresh");
            return new ReportState();
        }
    }

    private async Task SaveStateAsync(ReportState state, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(state, AbuseJsonContext.Default.ReportState);
        await File.WriteAllTextAsync(_stateFile, json, ct);
    }

    private sealed class IpAggregate
    {
        public HashSet<string> Baits { get; } = new();
        public int HitCount { get; set; }
        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;
    }
}

public sealed class ReportState
{
    [JsonPropertyName("last_reported")]
    public Dictionary<string, DateTimeOffset> LastReported { get; set; } = new();
}

[JsonSerializable(typeof(ReportState))]
internal sealed partial class AbuseJsonContext : JsonSerializerContext { }
