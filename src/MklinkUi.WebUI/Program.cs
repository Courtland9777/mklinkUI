using MklinkUi.Core;
using MklinkUi.WebUI;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using System.IO;
using System.Net;
using System.Net.Sockets;

var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
              ?? Environments.Production;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = envName
});

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddEnvironmentVariables("MKLINKUI__");

var logDir = builder.Configuration.GetValue<string>("Paths:LogDirectory");
if (string.IsNullOrWhiteSpace(logDir))
{
    logDir = OperatingSystem.IsWindows()
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MklinkUi", "logs")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ?? "/", ".mklinkui", "logs");
}
Directory.CreateDirectory(logDir);
builder.Configuration["Paths:LogDirectory"] = logDir;

builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .Enrich.WithProcessId()
       .Enrich.WithThreadId()
       .Enrich.WithProperty("ApplicationVersion", typeof(Program).Assembly.GetName().Version)
       .Enrich.WithProperty("Environment", envName)
       .WriteTo.File(Path.Combine(logDir, "log-.txt"), rollingInterval: RollingInterval.Day)
       .WriteTo.Console();
});

builder.Services.AddRazorPages();
builder.Services.AddPlatformServices();
builder.Services.AddSingleton<SymlinkManager>();

builder.Services.AddOptions<ServerOptions>()
    .Bind(builder.Configuration.GetSection("Server"))
    .ValidateDataAnnotations()
    .Validate(o => ValidateServerOptions(o), "Invalid server configuration")
    .ValidateOnStart();

builder.Services.AddOptions<SymlinkOptions>()
    .Bind(builder.Configuration.GetSection("Symlink"))
    .ValidateDataAnnotations();

builder.Services.AddOptions<UiOptions>()
    .Bind(builder.Configuration.GetSection("UI"))
    .ValidateDataAnnotations();

builder.Services.AddOptions<PathOptions>()
    .Bind(builder.Configuration.GetSection("Paths"))
    .PostConfigure(options =>
    {
        if (string.IsNullOrWhiteSpace(options.LogDirectory))
        {
            options.LogDirectory = OperatingSystem.IsWindows()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MklinkUi", "logs")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ?? "/", ".mklinkui", "logs");
        }
    });

var serverOpts = builder.Configuration.GetRequiredSection("Server").Get<ServerOptions>()!;
var range = ParseRange(serverOpts.PreferredPortRange);
var httpPort = FindAvailablePort(serverOpts.DefaultHttpPort, range);
var httpsPort = FindAvailablePort(serverOpts.DefaultHttpsPort, range);

builder.Configuration["Kestrel:Endpoints:Http:Url"] ??= $"http://localhost:{httpPort}";
builder.Configuration["Kestrel:Endpoints:Https:Url"] ??= $"https://localhost:{httpsPort}";

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

var app = builder.Build();

if (!OperatingSystem.IsWindows() && !app.Environment.IsDevelopment())
{
    throw new PlatformNotSupportedException("MklinkUI is supported on Windows only outside development.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(feature?.Error, "Unhandled exception");
        var correlationId = context.Response.Headers["X-Correlation-ID"].FirstOrDefault();
        var detail = new ErrorDetail(ErrorCodes.Unexpected, "An unexpected error occurred.", null, correlationId);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(detail);
    });
});

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlationId))
        correlationId = Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program
{
    public static (int Start, int End) ParseRange(string range)
    {
        var parts = range.Split('-');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    public static int FindAvailablePort(int preferred, (int Start, int End) range)
    {
        if (IsFree(preferred)) return preferred;
        for (var p = range.Start; p <= range.End; p++)
        {
            if (IsFree(p)) return p;
        }
        return preferred;
    }

    public static bool ValidateServerOptions(ServerOptions options)
    {
        var (start, end) = ParseRange(options.PreferredPortRange);
        return start > 0 && end <= 65535 && start <= end &&
            options.DefaultHttpPort >= start && options.DefaultHttpPort <= end &&
            options.DefaultHttpsPort >= start && options.DefaultHttpsPort <= end;
    }

    private static bool IsFree(int port)
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
}
