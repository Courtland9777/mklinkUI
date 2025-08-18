using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace MklinkUi.Core;

/// <summary>
///     Coordinates symbolic link creation using provided services.
/// </summary>
public sealed class SymlinkManager(
    IHostEnvironment environment,
    ISymlinkService symlinkService,
    IOptions<SymlinkOptions> options,
    ILogger<SymlinkManager> logger)
{
    private readonly SymlinkOptions _options = options.Value;
    /// <summary>
    ///     Creates a file symbolic link inside the destination folder if developer mode is enabled.
    /// </summary>
    public async Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

        if (!Path.IsPathFullyQualified(sourceFile) || !Path.IsPathFullyQualified(destinationFolder))
        {
            using var scope = logger.BeginScope(new Dictionary<string, object> { ["ErrorCode"] = ErrorCodes.InvalidPath });
            logger.LogWarning("Paths must be absolute. Source: {SourceFile}, Destination: {DestinationFolder}", sourceFile, destinationFolder);
            return new SymlinkResult(false, "Paths must be absolute.", ErrorCodes.InvalidPath);
        }

        if (!environment.IsDevelopment())
        {
            using var scope = logger.BeginScope(new Dictionary<string, object> { ["ErrorCode"] = ErrorCodes.DevModeRequired });
            logger.LogWarning("Developer mode not enabled.");
            return new SymlinkResult(false, "Developer mode not enabled.", ErrorCodes.DevModeRequired);
        }

        try
        {
            return await symlinkService.CreateFileLinkAsync(sourceFile, destinationFolder, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            using var scope = logger.BeginScope(new Dictionary<string, object> { ["ErrorCode"] = ErrorCodes.Unexpected });
            logger.LogError(ex, "Failed to create file link for {SourceFile} in {Destination}", sourceFile, destinationFolder);
            return new SymlinkResult(false, ex.Message, ErrorCodes.Unexpected);
        }
    }

    /// <summary>
    ///     Creates directory symbolic links for each source folder inside the destination folder if developer mode is enabled.
    /// </summary>
    public async Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IEnumerable<string> sourceFolders,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFolders);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);

        var sources = sourceFolders.ToList();

        if (sources.Any(s => string.IsNullOrWhiteSpace(s) || !Path.IsPathFullyQualified(s)) ||
            !Path.IsPathFullyQualified(destinationFolder))
        {
            using var scope = logger.BeginScope(new Dictionary<string, object> { ["ErrorCode"] = ErrorCodes.InvalidPath });
            logger.LogWarning("Paths must be absolute. Destination: {DestinationFolder}", destinationFolder);
            return [.. sources.Select(_ => new SymlinkResult(false, "Paths must be absolute.", ErrorCodes.InvalidPath))];
        }

        if (!environment.IsDevelopment())
        {
            using var scope = logger.BeginScope(new Dictionary<string, object> { ["ErrorCode"] = ErrorCodes.DevModeRequired });
            logger.LogWarning("Developer mode not enabled.");
            return [.. sources.Select(_ => new SymlinkResult(false, "Developer mode not enabled.", ErrorCodes.DevModeRequired))];
        }

        if (sources.Count > _options.BatchMax)
        {
            return [.. sources.Select(_ => new SymlinkResult(false, $"Too many items. Max {_options.BatchMax}"))];
        }

        var groups = sources
            .Select((s, i) => (Source: s, Index: i, Name: Path.GetFileName(s)))
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

        var unique = new List<(string Source, int Index)>();
        var results = new SymlinkResult[sources.Count];

        foreach (var group in groups)
        {
            var first = group.First();
            unique.Add((first.Source, first.Index));
            foreach (var item in group.Skip(1))
                results[item.Index] = new SymlinkResult(false, $"Duplicate folder name: {group.Key}");
        }

        try
        {
            var serviceResults = await symlinkService
                .CreateDirectoryLinksAsync(unique.Select(u => u.Source), destinationFolder, cancellationToken)
                .ConfigureAwait(false);

            if (serviceResults.Count != unique.Count)
            {
                logger.LogError("Symlink service returned {ResultCount} results for {SourceCount} sources.",
                    serviceResults.Count, unique.Count);
                var error = "Failed to create symlinks.";
                foreach (var (_, index) in unique)
                    results[index] = new SymlinkResult(false, error);
                return results;
            }

            for (var i = 0; i < unique.Count; i++)
                results[unique[i].Index] = serviceResults[i];

            return results;
        }
        catch (Exception ex)
        {
            using var scope = logger.BeginScope(new Dictionary<string, object> { ["ErrorCode"] = ErrorCodes.Unexpected });
            logger.LogError(ex, "Failed to create directory links in {DestinationFolder}", destinationFolder);
            var error = ex.Message;
            foreach (var (_, index) in unique)
                results[index] = new SymlinkResult(false, error, ErrorCodes.Unexpected);
            return results;
        }
    }
}