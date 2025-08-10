using System.Reflection;
using System.Runtime.InteropServices;
using MklinlUi.Core;

namespace MklinlUi.WebUI;

public static class ServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var assemblyName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "MklinlUi.Windows.dll"
            : "MklinlUi.Fakes.dll";
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName);

        var (dev, sym) = TryLoadServices(assemblyPath);

        services.AddSingleton(dev);
        services.AddSingleton(sym);

        return services;
    }

    private static (IDeveloperModeService dev, ISymlinkService sym) TryLoadServices(string assemblyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

        try
        {
            if (File.Exists(assemblyPath))
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var dev = Create<IDeveloperModeService>(assembly);
                var sym = Create<ISymlinkService>(assembly);
                if (dev != null && sym != null)
                    return (dev, sym);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load {assemblyPath}: {ex.Message}");
        }

        return (new DefaultDeveloperModeService(), new DefaultSymlinkService());

        static T? Create<T>(Assembly assembly) where T : class
        {
            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
            return type is not null ? Activator.CreateInstance(type) as T : null;
        }
    }

    private sealed class DefaultDeveloperModeService : IDeveloperModeService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
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

