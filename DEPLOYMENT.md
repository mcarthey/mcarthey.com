# Deployment Guide for SmarterASP.NET

## Issue: ASP.NET Core on IIS/SmarterASP.NET

ASP.NET Core applications require specific configuration to run on IIS-based hosting providers like SmarterASP.NET.

## Critical Files for Deployment

### 1. web.config ✅ (Now Included)
The `web.config` file tells IIS how to run your ASP.NET Core application using the ASP.NET Core Module.

**Key settings:**
- Uses `AspNetCoreModuleV2`
- Points to `mcarthey.com.dll`
- Sets hosting model to `inprocess` (faster)
- Enables detailed error messages for troubleshooting

### 2. Updated .csproj ✅ (Now Configured)
Added deployment-specific settings:
- `AspNetCoreHostingModel`: InProcess
- Web.config copy behavior
- Publish configuration

## Deployment Steps for SmarterASP.NET

### Option A: Visual Studio Publish (Recommended)

1. **Open the solution in Visual Studio**
2. **Right-click the project** → Select "Publish"
3. **Use the SmarterASP profile** (Properties/PublishProfiles/SmarterASP.pubxml)
4. **Click Publish** - outputs to `bin\Release\net8.0\publish\`
5. **Upload contents** of the publish folder via FTP to your SmarterASP.NET site

### Option B: Command Line Publish

```bash
# Navigate to project directory
cd /path/to/mcarthey.com

# Publish the application
dotnet publish -c Release -o ./publish

# Upload contents of ./publish folder to SmarterASP.NET via FTP
```

### Option C: Manual FTP Upload (Current Files)

If you've already built locally and it works, upload these files:
- All `.dll` files from bin/Release/net8.0/
- `web.config` (CRITICAL - this is the fix!)
- `appsettings.json`
- `wwwroot/` folder (all static files)
- All View files (if using Razor runtime compilation)

## Troubleshooting SmarterASP.NET Deployment

### Problem 1: "500.19 Internal Server Error" or Blank Page
**Cause**: Missing or incorrect `web.config`
**Solution**: ✅ web.config is now included - redeploy with this file

### Problem 2: "500.30 ASP.NET Core app failed to start"
**Causes**:
- ASP.NET Core Runtime not installed on server
- Wrong .NET version targeted

**Solutions**:
1. **Check SmarterASP.NET's .NET Core support**:
   - Log into SmarterASP.NET control panel
   - Check which ASP.NET Core versions are available
   - They should support .NET 8, but if not, we can downgrade to .NET 6

2. **Verify Application Pool Settings**:
   - In IIS Manager (if you have access via SmarterASP control panel)
   - Set .NET CLR Version to: **"No Managed Code"**
   - This is CRITICAL for ASP.NET Core apps

3. **Check web.config paths**:
   - Ensure `processPath="dotnet"` is correct
   - Ensure `arguments=".\mcarthey.com.dll"` matches your DLL name

### Problem 3: Static Files Not Loading (CSS/JS/Images)
**Cause**: Middleware order or missing MIME types
**Solutions**:
- ✅ web.config includes MIME type mappings
- ✅ Program.cs has `app.UseStaticFiles()` before routing

### Problem 4: Views Not Found
**Causes**:
- Views folder not published
- Case sensitivity on Linux servers

**Solutions**:
- Ensure Views/ folder is uploaded
- Check file extensions (.cshtml)
- Verify _ViewStart.cshtml and _ViewImports.cshtml are present

### Problem 5: Wrong .NET Version on Server
If SmarterASP.NET doesn't support .NET 8 yet, downgrade:

```xml
<!-- Change in mcarthey.com.csproj -->
<TargetFramework>net8.0</TargetFramework>
<!-- to -->
<TargetFramework>net6.0</TargetFramework>
```

## SmarterASP.NET Specific Configuration

### Control Panel Settings to Verify:

1. **Application Pool Settings**:
   - Navigate to: Control Panel → IIS Manager → Application Pools
   - Select your application pool
   - Set ".NET CLR Version" to **"No Managed Code"**
   - Set "Managed Pipeline Mode" to **"Integrated"**

2. **ASP.NET Core Module**:
   - Verify ASP.NET Core Hosting Bundle is installed
   - Should see AspNetCoreModuleV2 in IIS modules
   - If not, contact SmarterASP.NET support

3. **File Permissions**:
   - Ensure IIS_IUSRS has read/execute permissions
   - wwwroot folder needs read permissions

4. **Enable Detailed Errors** (Temporarily):
   In web.config, already set:
   ```xml
   <httpErrors errorMode="Detailed" />
   ```

   In Program.cs, check environment:
   ```csharp
   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
   }
   ```

## Quick Deployment Checklist

- [x] web.config created with AspNetCoreModuleV2
- [x] .csproj configured for publish
- [x] Application compiled in Release mode
- [ ] All files uploaded via FTP to SmarterASP.NET
- [ ] IIS Application Pool set to "No Managed Code"
- [ ] ASP.NET Core 8 Runtime available on server (or downgrade to 6)
- [ ] web.config points to correct DLL name
- [ ] wwwroot folder uploaded with all static files
- [ ] Views folder uploaded with all .cshtml files

## Testing After Deployment

1. **Visit the site** - should see terminal animation
2. **Check browser console** - look for 404s on static files
3. **Test navigation** - click ABOUT, PROJECTS, HOBBIES
4. **Check images** - verify they load correctly

## If Still Not Working

### Get Detailed Error Information:

1. **Enable stdout logging** in web.config:
   ```xml
   <aspNetCore stdoutLogEnabled="true"
               stdoutLogFile=".\logs\stdout" ...>
   ```

2. **Create logs folder** on server via FTP

3. **Check error logs** - download and review

4. **Contact SmarterASP.NET Support** with:
   - Error logs from stdout
   - Screenshot of IIS Application Pool settings
   - Confirm ASP.NET Core 8 is installed

## Alternative: Downgrade to .NET 6 LTS

If SmarterASP.NET doesn't support .NET 8 yet:

```bash
# 1. Update .csproj
<TargetFramework>net6.0</TargetFramework>

# 2. Rebuild and publish
dotnet publish -c Release -f net6.0

# 3. Update web.config if needed (usually automatic)
```

.NET 6 is LTS (Long Term Support) and widely supported by hosting providers.

## Success Indicators

When working correctly, you should see:
- ✅ Terminal boot sequence (or skip button)
- ✅ Navigation ASCII art blocks
- ✅ Command input working
- ✅ All pages load (About, Projects, Hobbies)
- ✅ Images display
- ✅ CSS styling applied (green on black, CRT effects)

## Need More Help?

If these steps don't resolve the issue, gather:
1. Exact error message (if any)
2. Screenshot of what you see
3. SmarterASP.NET control panel screenshots
4. Contents of error logs (if available)

Then contact SmarterASP.NET support or reply with details.
