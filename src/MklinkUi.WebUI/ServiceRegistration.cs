using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MklinkUi.Core;
using Microsoft.Extensions.Logging;

namespace MklinkUi.WebUI;

public static class ServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ServicesWrapper>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ServiceRegistration");
            var (dev, sym) = LoadServices(logger);
            return new ServicesWrapper(dev, sym);
        });

        services.AddSingleton<IDeveloperModeService>(sp =>
            sp.GetRequiredService<ServicesWrapper>().Dev);

        services.AddSingleton<ISymlinkService>(sp =>
            sp.GetRequiredService<ServicesWrapper>().Sym);

        return services;

        static (IDeveloperModeService dev, ISymlinkService sym) LoadServices(ILogger logger)
        {
            var assemblyName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "MklinkUi.Windows.dll"
                : "MklinkUi.Fakes.dll";
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName);
            return TryLoadServices(assemblyPath, logger);
        }
    }

    private sealed record ServicesWrapper(IDeveloperModeService Dev, ISymlinkService Sym);

    private static (IDeveloperModeService dev, ISymlinkService sym) TryLoadServices(string assemblyPath,
        ILogger logger)
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
            var dev = Create<IDeveloperModeService>(assembly);
            var sym = Create<ISymlinkService>(assembly);
            if (dev != null && sym != null)
                return (dev, sym);

            throw new InvalidOperationException($"Required services not found in {assemblyPath}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load {AssemblyPath}", assemblyPath);
            return (new DefaultDeveloperModeService(), new DefaultSymlinkService());
        }

        static T? Create<T>(Assembly assembly) where T : class
        {
            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
            return type is not null ? Activator.CreateInstance(type) as T : null;
        }
    }

    private sealed class DefaultDeveloperModeService : IDeveloperModeService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (!OperatingSystem.IsWindows())
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(string.Equals(
                Environment.GetEnvironmentVariable("MKLINKUI_DEVELOPER_MODE"),
                "true", StringComparison.OrdinalIgnoreCase));
        }
    }

    private sealed class DefaultSymlinkService : ISymlinkService
    {
        public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(linkPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

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

            var results = new List<SymlinkResult>();

            foreach (var source in sourceFiles)
            {
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

