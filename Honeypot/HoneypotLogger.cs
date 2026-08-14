using System.Text.Json;
using System.Threading.Channels;

namespace mcarthey.com.Honeypot;

// Append-only JSONL logger with a background writer.
// Requests enqueue entries into an unbounded channel; a single background
// task drains the channel and writes to disk. Keeps request threads off
// disk I/O and serializes writes without lock contention.
public sealed class HoneypotLogger : IHostedService, IAsyncDisposable
{
    private readonly Channel<HoneypotEntry> _channel = Channel.CreateUnbounded<HoneypotEntry>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });
    private readonly string _logDir;
    private readonly ILogger<HoneypotLogger> _log;
    private Task? _writer;
    private CancellationTokenSource? _cts;

    public HoneypotLogger(IConfiguration config, ILogger<HoneypotLogger> log)
    {
        _logDir = config["Honeypot:LogDirectory"] ?? "logs/honeypot";
        _log = log;
    }

    public void Enqueue(HoneypotEntry entry) => _channel.Writer.TryWrite(entry);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_logDir);
        _cts = new CancellationTokenSource();
        _writer = Task.Run(() => WriteLoopAsync(_cts.Token));
        _log.LogInformation("Honeypot logger writing to {LogDir}", _logDir);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        _cts?.Cancel();
        if (_writer is not null)
        {
            try { await _writer.WaitAsync(cancellationToken); } catch (OperationCanceledException) { }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);
        _cts?.Dispose();
    }

    private async Task WriteLoopAsync(CancellationToken ct)
    {
        var reader = _channel.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            // Drain in a batch per day to minimize file open/close.
            var byDay = new Dictionary<string, List<string>>();
            while (reader.TryRead(out var entry))
            {
                var day = entry.Timestamp.ToString("yyyy-MM-dd");
                var line = JsonSerializer.Serialize(entry, HoneypotJsonContext.Default.HoneypotEntry);
                if (!byDay.TryGetValue(day, out var list)) byDay[day] = list = new List<string>();
                list.Add(line);
            }
            foreach (var (day, lines) in byDay)
            {
                var path = Path.Combine(_logDir, $"{day}.jsonl");
                try
                {
                    await File.AppendAllLinesAsync(path, lines, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Honeypot log write failed to {Path}", path);
                }
            }
        }
    }
}

public sealed record HoneypotEntry(
    DateTimeOffset Timestamp,
    string Bait,
    string RemoteIp,
    string Method,
    string Path,
    string? QueryString,
    string? UserAgent,
    string? Referer,
    Dictionary<string, string> Headers,
    string? BodySnippet,
    int BodyLength);

[System.Text.Json.Serialization.JsonSerializable(typeof(HoneypotEntry))]
internal sealed partial class HoneypotJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
}
