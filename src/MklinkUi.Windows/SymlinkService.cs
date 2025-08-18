using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MklinkUi.Core;

namespace MklinkUi.Windows;

/// <summary>
///     Windows implementation of <see cref="ISymlinkService"/>.
/// </summary>
public sealed class SymlinkService : ISymlinkService
{
    private readonly ILogger<SymlinkService> _logger;

    public SymlinkService() : this(null)
    {
    }

    public SymlinkService(ILogger<SymlinkService>? logger)
    {
        _logger = logger ?? NullLogger<SymlinkService>.Instance;
    }

    public async Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield(); // keep method truly async; allows cooperative cancellation
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(sourceFile) || string.IsNullOrWhiteSpace(destinationFolder))
            throw new ArgumentException("Source and destination are required.");

        if (!Path.IsPathFullyQualified(sourceFile) || !Path.IsPathFullyQualified(destinationFolder))
            throw new ArgumentException("Paths must be absolute.");

        if (!File.Exists(sourceFile))
        {
            _logger.LogWarning("Source file not found: {Source}", sourceFile);
            return new SymlinkResult(false, "Path not found.", ErrorCodes.PathNotFound);
        }

        if (!Directory.Exists(destinationFolder))
        {
            _logger.LogWarning("Destination folder not found: {Destination}", destinationFolder);
            return new SymlinkResult(false, "Path not found.", ErrorCodes.PathNotFound);
        }

        var link = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
        if (File.Exists(link) || Directory.Exists(link))
        {
            _logger.LogInformation("Link already exists, skipping: {Link}", link);
            return new SymlinkResult(false, "Link already exists.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            File.CreateSymbolicLink(link, sourceFile);
            _logger.LogInformation("Created file symlink: {Link} -> {Source}", link, sourceFile);
            return new SymlinkResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file link for {Source} in {Destination}", sourceFile, destinationFolder);
            return new SymlinkResult(false, GetMessage(ex));
        }
    }

    public Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IEnumerable<string> sourceFolders,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFolders);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);
        if (!PathHelpers.IsFullyQualified(destinationFolder))
            throw new ArgumentException("Paths must be absolute.");

        var results = new List<SymlinkResult>();
        foreach (var source in sourceFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!PathHelpers.IsFullyQualified(source))
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
                Directory.CreateSymbolicLink(link, source);
                results.Add(new SymlinkResult(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create directory link from {Source} to {Link}", source, link);
                results.Add(new SymlinkResult(false, GetMessage(ex)));
            }
        }

        return Task.FromResult<IReadOnlyList<SymlinkResult>>(results);
    }

    private static string GetMessage(Exception ex) => ex switch
    {
        UnauthorizedAccessException => "Access denied.",
        DirectoryNotFoundException or FileNotFoundException => "Path not found.",
        IOException => "I/O error occurred while creating the link.",
        _ => "Unexpected error occurred."
    };
}
