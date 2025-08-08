using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using MklinlUi.Core;

namespace MklinlUi.WebUI;

public static class ServiceRegistration
{
    public static void AddPlatformServices(this IServiceCollection services)
    {
        string assemblyFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "MklinlUi.Windows.dll"
            : "MklinlUi.Fakes.dll";
        string assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyFile);
        Type? devType = null;
        Type? symType = null;
        try
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            devType = assembly.GetTypes().FirstOrDefault(t => typeof(IDeveloperModeService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            symType = assembly.GetTypes().FirstOrDefault(t => typeof(ISymlinkService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load {assemblyPath}: {ex.Message}");
        }

        if (devType != null && Activator.CreateInstance(devType) is IDeveloperModeService devInstance)
        {
            services.AddSingleton<IDeveloperModeService>(devInstance);
        }
        else
        {
            services.AddSingleton<IDeveloperModeService>(new DefaultDeveloperModeService());
        }

        if (symType != null && Activator.CreateInstance(symType) is ISymlinkService symInstance)
        {
            services.AddSingleton<ISymlinkService>(symInstance);
        }
        else
        {
            services.AddSingleton<ISymlinkService>(new DefaultSymlinkService());
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
