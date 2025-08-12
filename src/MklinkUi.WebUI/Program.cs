using MklinkUi.Core;
using MklinkUi.WebUI;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddRazorPages();
builder.Services.AddPlatformServices();
builder.Services.AddSingleton<SymlinkManager>();

var configuredUrls = builder.Configuration["ASPNETCORE_URLS"];
if (string.IsNullOrWhiteSpace(configuredUrls))
{
    var httpPort = builder.Configuration.GetValue("Server:Port", 5280);
    if (builder.Configuration.GetSection("Kestrel:Certificates:Default").Exists())
    {
        var httpsPort = builder.Configuration.GetValue("Server:HttpsPort", 5281);
        builder.WebHost.UseUrls($"http://localhost:{httpPort}", $"https://localhost:{httpsPort}");
    }
    else
    {
        builder.WebHost.UseUrls($"http://localhost:{httpPort}");
    }
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
