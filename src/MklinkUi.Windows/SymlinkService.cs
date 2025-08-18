using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MklinkUi.Core;
using System.Linq;

namespace MklinkUi.Windows;

/// <summary>
///     Windows implementation of <see cref="ISymlinkService"/>.
/// </summary>
public sealed class SymlinkService : ISymlinkService
{
    private readonly ILogger<SymlinkService> _logger;
    private readonly IDeveloperModeService _developerMode;

    public SymlinkService(IDeveloperModeService? developerModeService = null, ILogger<SymlinkService>? logger = null)
    {
        _developerMode = developerModeService ?? new StubDeveloperModeService();
        _logger = logger ?? NullLogger<SymlinkService>.Instance;
    }

    public async Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        sourceFile = PathValidation.EnsureAbsolute(sourceFile, nameof(sourceFile));
        destinationFolder = PathValidation.EnsureAbsolute(destinationFolder, nameof(destinationFolder));

        if (!_developerMode.IsEnabled)
            return new SymlinkResult(false, "Developer mode not enabled.", ErrorCodes.DevModeRequired);

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
        link = Path.GetFullPath(link);
        if (File.Exists(link) || Directory.Exists(link))
        {
            _logger.LogInformation("Link already exists, skipping: {Link}", link);
            return new SymlinkResult(false, "Link already exists.", ErrorCodes.AlreadyExists);
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

    public async Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IReadOnlyList<string> sourceFolders,
        string destinationFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceFolders);
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        destinationFolder = PathValidation.EnsureAbsolute(destinationFolder, nameof(destinationFolder));

        if (!_developerMode.IsEnabled)
        {
            var fails = Enumerable.Repeat(new SymlinkResult(false, "Developer mode not enabled.", ErrorCodes.DevModeRequired), sourceFolders.Count).ToArray();
            return fails;
        }

        var results = new List<SymlinkResult>(sourceFolders.Count);
        foreach (var source in sourceFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string absSource;
            try
            {
                absSource = PathValidation.EnsureAbsolute(source, nameof(sourceFolders));
            }
            catch (ArgumentException)
            {
                results.Add(new SymlinkResult(false, "Paths must be absolute.", ErrorCodes.InvalidPath));
                continue;
            }

            var link = Path.Combine(destinationFolder, Path.GetFileName(absSource));
            link = Path.GetFullPath(link);
            if (File.Exists(link) || Directory.Exists(link))
            {
                results.Add(new SymlinkResult(false, "Link already exists.", ErrorCodes.AlreadyExists));
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                Directory.CreateSymbolicLink(link, absSource);
                results.Add(new SymlinkResult(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create directory link from {Source} to {Link}", absSource, link);
                results.Add(new SymlinkResult(false, GetMessage(ex)));
            }
        }

        return results;
    }

    private static string GetMessage(Exception ex) => ex switch
    {
        UnauthorizedAccessException => "Access denied.",
        DirectoryNotFoundException or FileNotFoundException => "Path not found.",
        IOException => "I/O error occurred while creating the link.",
        _ => "Unexpected error occurred."
    };

    private sealed class StubDeveloperModeService : IDeveloperModeService
    {
        public bool IsEnabled => true;
    }
}
