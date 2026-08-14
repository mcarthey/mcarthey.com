using mcarthey.com.Honeypot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<HoneypotLogger>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<HoneypotLogger>());
builder.Services.AddSingleton<HoneypotStats>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Honeypot BEFORE HTTPS redirect + routing. Many bots don't follow 307s;
// putting the trap ahead of the redirect captures them either way. Real
// human traffic to Home/About/Projects/Hobbies never touches bait paths,
// so the redirect still promotes those requests normally below.
app.UseMiddleware<HoneypotMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
