using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using MklinlUi.Core;

namespace MklinlUi.WebUI;

public static class ServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
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
        try
        {
            if (File.Exists(assemblyPath))
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var dev = Create<IDeveloperModeService>(assembly);
                var sym = Create<ISymlinkService>(assembly);
                if (dev != null && sym != null)
                {
                    return (dev, sym);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load {assemblyPath}: {ex.Message}");
        }

        return (new DefaultDeveloperModeService(), new DefaultSymlinkService());

        static T? Create<T>(Assembly assembly) where T : class
        {
            var type = assembly.GetTypes().FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            return type is not null ? Activator.CreateInstance(type) as T : null;
        }
    }

    private sealed class DefaultDeveloperModeService : IDeveloperModeService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class DefaultSymlinkService : ISymlinkService
    {
        public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Directory.Exists(targetPath))
                {
                    Directory.CreateSymbolicLink(linkPath, targetPath);
                }
                else
                {
                    File.CreateSymbolicLink(linkPath, targetPath);
                }

                return Task.FromResult(new SymlinkResult(true));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new SymlinkResult(false, ex.Message));
            }
        }
    }
}

