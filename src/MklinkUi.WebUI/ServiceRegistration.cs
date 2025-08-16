using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MklinkUi.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MklinkUi.WebUI;

public static class ServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ServicesWrapper>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ServiceRegistration");
            var config = sp.GetRequiredService<IConfiguration>();
            var (dev, sym) = LoadServices(sp, logger, config);
            return new ServicesWrapper(dev, sym);
        });

        services.AddSingleton<IDeveloperModeService>(sp =>
            sp.GetRequiredService<ServicesWrapper>().Dev);

        services.AddSingleton<ISymlinkService>(sp =>
            sp.GetRequiredService<ServicesWrapper>().Sym);

        return services;

        static (IDeveloperModeService dev, ISymlinkService sym) LoadServices(IServiceProvider sp, ILogger logger, IConfiguration config)
        {
            var assemblyName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "MklinkUi.Windows.dll"
                : "MklinkUi.Fakes.dll";
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName);
            return TryLoadServices(sp, assemblyPath, logger, config);
        }
    }

    private sealed record ServicesWrapper(IDeveloperModeService Dev, ISymlinkService Sym);

    private static (IDeveloperModeService dev, ISymlinkService sym) TryLoadServices(IServiceProvider sp,
        string assemblyPath, ILogger logger, IConfiguration config)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException("Service assembly not found.", assemblyPath);
            }

            var assembly = Assembly.LoadFrom(assemblyPath);
            var dev = Create<IDeveloperModeService>(assembly, sp);
            var sym = Create<ISymlinkService>(assembly, sp);
            if (dev != null && sym != null)
                return (dev, sym);

            throw new InvalidOperationException($"Required services not found in {assemblyPath}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load {AssemblyPath}", assemblyPath);
            return (new DefaultDeveloperModeService(logger, config), new DefaultSymlinkService());
        }

        static T? Create<T>(Assembly assembly, IServiceProvider sp) where T : class
        {
            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
            return type is not null ? ActivatorUtilities.CreateInstance(sp, type) as T : null;
        }
    }

    private sealed class DefaultDeveloperModeService : IDeveloperModeService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public DefaultDeveloperModeService(ILogger logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = _config["DeveloperMode"] ?? Environment.GetEnvironmentVariable("MKLINKUI_DEVELOPER_MODE");

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (bool.TryParse(value, out var parsedBool))
                    return Task.FromResult(parsedBool);

                if (int.TryParse(value, out var parsedInt))
                    return Task.FromResult(parsedInt != 0);

                _logger.LogWarning("Developer mode value '{Value}' is invalid. Defaulting to disabled.", value);
            }
            else
            {
                _logger.LogWarning("Developer mode value is not set. Developer mode status cannot be determined.");
            }

            return Task.FromResult(false);
        }
    }

    private sealed class DefaultSymlinkService : ISymlinkService
    {
        public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (Directory.Exists(targetPath))
                    Directory.CreateSymbolicLink(linkPath, targetPath);
                else
                    File.CreateSymbolicLink(linkPath, targetPath);

                return Task.FromResult(new SymlinkResult(true));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new SymlinkResult(false, ex.Message));
            }
        }

        public Task<IReadOnlyList<SymlinkResult>> CreateFileSymlinksAsync(IEnumerable<string> sourceFiles,
            string destinationFolder, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceFiles);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

            cancellationToken.ThrowIfCancellationRequested();

            var results = new List<SymlinkResult>();

            foreach (var source in sourceFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(source))
                {
                    results.Add(new SymlinkResult(false, "Invalid source."));
                    continue;
                }

                var link = Path.Combine(destinationFolder, Path.GetFileName(source));

                if (File.Exists(link) || Directory.Exists(link))
                {
                    results.Add(new SymlinkResult(false, "Link already exists."));
                    continue;
                }

                try
                {
                    File.CreateSymbolicLink(link, source);
                    results.Add(new SymlinkResult(true));
                }
                catch (Exception ex)
                {
                    results.Add(new SymlinkResult(false, ex.Message));
                }
            }

            return Task.FromResult((IReadOnlyList<SymlinkResult>)results);
        }
    }
}

