using mcarthey.com.Honeypot;
using Microsoft.AspNetCore.Mvc;

namespace mcarthey.com.Controllers;

public class ShameController : Controller
{
    private readonly HoneypotStats _stats;
    public ShameController(HoneypotStats stats) => _stats = stats;

    // 60s response cache -- the underlying HoneypotStats already caches for 60s,
    // this pushes the cache to the client + intermediaries so bot swarms hitting
    // /shame don't cause file re-scans on every request.
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var data = await _stats.GetAsync(ct);
        return View(data);
    }
}
