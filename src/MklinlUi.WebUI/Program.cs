using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using MklinlUi.Core;
using MklinlUi.WebUI;

var builder = WebApplication.CreateBuilder(args);

using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

static bool PortAvailable(int port, ILogger logger)
{
    try
    {
        using var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch (SocketException ex)
    {
        logger.LogError(ex, "Port {Port} is unavailable", port);
        return false;
    }
}

static int FindPort(int start, int end, ILogger logger)
{
    for (var p = start; p <= end; p++)
    {
        if (PortAvailable(p, logger)) return p;
    }
    var message = $"No available ports between {start} and {end}.";
    logger.LogError(message);
    throw new InvalidOperationException(message);
}

builder.Services.AddRazorPages();
builder.Services.AddPlatformServices();
builder.Services.AddSingleton<SymlinkManager>();

var configuredUrls = builder.Configuration["ASPNETCORE_URLS"];
if (string.IsNullOrWhiteSpace(configuredUrls))
{
    var httpPort = builder.Configuration.GetValue<int?>("Server:Port") ?? FindPort(5280, 5299, logger);
    if (builder.Configuration.GetSection("Kestrel:Certificates:Default").Exists())
    {
        var httpsPort = FindPort(5281, 5299, logger);
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
