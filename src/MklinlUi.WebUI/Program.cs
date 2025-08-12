using System.Net;
using System.Net.Sockets;
using MklinlUi.Core;
using MklinlUi.WebUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

static bool PortAvailable(int port)
{
    try
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch
    {
        return false;
    }
}

static int FindPort(int start, int end)
{
    for (var p = start; p <= end; p++)
    {
        if (PortAvailable(p)) return p;
    }
    return start;
}

builder.Services.AddRazorPages();
using var serviceProvider = builder.Services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("ServiceRegistration");
builder.Services.AddPlatformServices(logger);
builder.Services.AddSingleton<SymlinkManager>();

var configuredUrls = builder.Configuration["ASPNETCORE_URLS"];
if (string.IsNullOrWhiteSpace(configuredUrls))
{
    var httpPort = builder.Configuration.GetValue<int?>("Server:Port") ?? FindPort(5280, 5299);
    if (builder.Configuration.GetSection("Kestrel:Certificates:Default").Exists())
    {
        var httpsPort = FindPort(5281, 5299);
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
