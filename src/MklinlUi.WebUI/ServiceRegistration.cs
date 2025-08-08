using MklinlUi.Core;
#if WINDOWS
using MklinlUi.Windows;
#endif

namespace MklinlUi.WebUI;

public static class ServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IDeveloperModeService, DeveloperModeService>();
            services.AddSingleton<ISymlinkService, SymlinkService>();
            return services;
        }
#endif
        services.AddSingleton<IDeveloperModeService, DefaultDeveloperModeService>();
        services.AddSingleton<ISymlinkService, DefaultSymlinkService>();
        return services;
    }

    private sealed class DefaultDeveloperModeService : IDeveloperModeService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class DefaultSymlinkService : ISymlinkService
    {
        public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath,
            CancellationToken cancellationToken = default)
        {
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
    }
}

