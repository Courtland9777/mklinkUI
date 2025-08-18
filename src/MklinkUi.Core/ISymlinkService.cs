namespace MklinkUi.Core;

/// <summary>
/// Provides functionality for creating symbolic links.
/// </summary>
public interface ISymlinkService
{
    /// <summary>
    /// Creates a file symbolic link inside the specified destination folder.
    /// The link name matches the source file name.
    /// </summary>
    /// <param name="sourceFile">Absolute path to the source file.</param>
    /// <param name="destinationFolder">Absolute path to the destination folder where the link will be created.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="SymlinkResult"/> describing the outcome.</returns>
    Task<SymlinkResult> CreateFileLinkAsync(string sourceFile, string destinationFolder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates directory symbolic links for each source folder inside the destination folder.
    /// Each link name matches its source folder name.
    /// </summary>
    /// <param name="sourceFolders">Absolute paths to the source folders.</param>
    /// <param name="destinationFolder">Absolute path to the destination folder.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="SymlinkResult"/> objects describing the outcome of each link.</returns>
    Task<IReadOnlyList<SymlinkResult>> CreateDirectoryLinksAsync(IReadOnlyList<string> sourceFolders,
        string destinationFolder, CancellationToken cancellationToken = default);
}
