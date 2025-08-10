namespace MklinlUi.Core;

/// <summary>
/// Provides functionality for creating symbolic links.
/// </summary>
public interface ISymlinkService
{
    /// <summary>
    /// Creates a symbolic link from <paramref name="linkPath"/> to <paramref name="targetPath"/>.
    /// </summary>
    /// <param name="linkPath">The path of the link to create.</param>
    /// <param name="targetPath">The target path the link should point to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="SymlinkResult"/> describing the outcome.</returns>
    Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates symbolic links for the specified <paramref name="sourceFiles"/> inside
    /// <paramref name="destinationFolder"/>. Each link name matches its source file name.
    /// </summary>
    /// <param name="sourceFiles">Paths to the source files.</param>
    /// <param name="destinationFolder">Folder where the links will be created.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="SymlinkResult"/> objects describing each link outcome.</returns>
    Task<IReadOnlyList<SymlinkResult>> CreateFileSymlinksAsync(IEnumerable<string> sourceFiles,
        string destinationFolder, CancellationToken cancellationToken = default);
}
