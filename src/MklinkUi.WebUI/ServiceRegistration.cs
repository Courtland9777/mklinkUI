using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MklinkUi.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MklinkUi.WebUI;

public static class ServiceRegistration
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ISymlinkService>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ServiceRegistration");
            var assemblyName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "MklinkUi.Windows.dll"
                : "MklinkUi.Fakes.dll";
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyName);
            var service = TryLoadService<ISymlinkService>(sp, assemblyPath, logger);
            return service ?? new DefaultSymlinkService(sp.GetRequiredService<IOptions<SymlinkOptions>>());
        });

        return services;
    }

    private static T? TryLoadService<T>(IServiceProvider sp, string assemblyPath, ILogger logger) where T : class
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
            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(T).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
            if (type is not null)
            {
                return ActivatorUtilities.CreateInstance(sp, type) as T;
            }

            throw new InvalidOperationException($"Required service not found in {assemblyPath}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load {AssemblyPath}", assemblyPath);
            return null;
        }
    }

    private sealed class DefaultSymlinkService : ISymlinkService
    {
        private readonly SymlinkOptions _options;

        public DefaultSymlinkService(IOptions<SymlinkOptions> options)
        {
            _options = options.Value;
        }
        public Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);
            cancellationToken.ThrowIfCancellationRequested();

            if (!Path.IsPathFullyQualified(sourceFile) || !Path.IsPathFullyQualified(destinationFolder))
                throw new ArgumentException("Paths must be absolute.");

            var link = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));

            try
            {
                var targetLink = HandleCollision(link, isDirectory: false);
                File.CreateSymbolicLink(targetLink, sourceFile);
                return Task.FromResult(new SymlinkResult(true));
            }
            catch (IOException ex) when (ex.Message == "Link already exists.")
            {
                return Task.FromResult(new SymlinkResult(false, ex.Message));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new SymlinkResult(false, ex.Message));
            }
        }

        public Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IEnumerable<string> sourceFolders,
            string destinationFolder, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceFolders);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

            cancellationToken.ThrowIfCancellationRequested();

            var results = new List<SymlinkResult>();

            foreach (var source in sourceFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(source) || !Path.IsPathFullyQualified(source) ||
                    !Path.IsPathFullyQualified(destinationFolder))
                {
                    results.Add(new SymlinkResult(false, "Invalid source."));
                    continue;
                }

                var link = Path.Combine(destinationFolder, Path.GetFileName(source));

                try
                {
                    var targetLink = HandleCollision(link, isDirectory: true);
                    Directory.CreateSymbolicLink(targetLink, source);
                    results.Add(new SymlinkResult(true));
                }
                catch (IOException ex) when (ex.Message == "Link already exists.")
                {
                    results.Add(new SymlinkResult(false, ex.Message));
                }
                catch (Exception ex)
                {
                    results.Add(new SymlinkResult(false, ex.Message));
                }
            }

            return Task.FromResult((IReadOnlyList<SymlinkResult>)results);
        }

        private string HandleCollision(string path, bool isDirectory)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                return path;

            switch (_options.CollisionPolicy)
            {
                case CollisionPolicy.Skip:
                    throw new IOException("Link already exists.");
                case CollisionPolicy.Overwrite:
                    if (isDirectory)
                        Directory.Delete(path, recursive: true);
                    else
                        File.Delete(path);
                    return path;
                case CollisionPolicy.Rename:
                    var basePath = path;
                    var counter = 1;
                    string candidate;
                    do
                    {
                        candidate = $"{basePath}.{counter++}";
                    } while (File.Exists(candidate) || Directory.Exists(candidate));
                    return candidate;
                default:
                    return path;
            }
        }
    }
}
