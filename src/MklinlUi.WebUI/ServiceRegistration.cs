using System.Reflection;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using MklinlUi.Core;

namespace MklinlUi.WebUI;

public static class ServiceRegistration
{
    public static void AddPlatformServices(this IServiceCollection services)
    {
        string assemblyName = OperatingSystem.IsWindows() ? "MklinlUi.Windows" : "MklinlUi.Fakes";
        try
        {
            var assembly = Assembly.Load(assemblyName);
            var devType = assembly.GetTypes().FirstOrDefault(t => typeof(IDeveloperModeService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            var symType = assembly.GetTypes().FirstOrDefault(t => typeof(ISymlinkService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            if (devType != null)
            {
                services.AddSingleton(typeof(IDeveloperModeService), devType);
            }
            if (symType != null)
            {
                services.AddSingleton(typeof(ISymlinkService), symType);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load {assemblyName}: {ex.Message}");
            services.AddSingleton<IDeveloperModeService, DefaultDeveloperModeService>();
            services.AddSingleton<ISymlinkService, DefaultSymlinkService>();
        }
    }

    private class DefaultDeveloperModeService : IDeveloperModeService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private class DefaultSymlinkService : ISymlinkService
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
