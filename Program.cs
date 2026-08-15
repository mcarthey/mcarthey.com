using mcarthey.com.Hack;
using mcarthey.com.Honeypot;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<HoneypotLogger>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<HoneypotLogger>());
builder.Services.AddSingleton<HoneypotStats>();
builder.Services.AddSingleton<GameSession>();

// Trust X-Forwarded-For and X-Forwarded-Proto from Caddy on localhost.
// Do NOT clear KnownIPNetworks -- the default includes loopback, which
// is exactly what we need (Caddy connects to Kestrel from 127.0.0.1).
// Kestrel binds localhost only in prod so external clients can't spoof.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// DataProtection with a persistent key ring -- signs the hack session cookie.
// Path resolves to /var/lib/mcarthey/dp-keys in prod (created by Ansible),
// ./dp-keys in dev.
var dpKeyPath = builder.Configuration["DataProtection:KeysPath"] ?? "dp-keys";
Directory.CreateDirectory(dpKeyPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeyPath))
    .SetApplicationName("mcarthey.com");

var app = builder.Build();

// MUST run before anything that inspects the request URL/IP -- makes
// ctx.Connection.RemoteIpAddress and ctx.Request.IsHttps reflect what
// the client actually did, not what Caddy proxied to Kestrel.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Honeypot BEFORE HTTPS redirect + routing. Many bots don't follow 307s;
// putting the trap ahead of the redirect captures them either way. Real
// human traffic never touches bait paths so the redirect still promotes
// those requests normally below.
app.UseMiddleware<HoneypotMiddleware>();

app.UseHttpsRedirection();

// Short max-age on static assets (CSS, JS, images). Long enough to be a
// real cache during a browsing session, short enough that any staleness
// self-heals within a minute or two after a deploy. Combined with the
// no-store policy on the HomeController this keeps browsers current
// without needing per-asset version query strings.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public,max-age=120,must-revalidate";
    }
});
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
