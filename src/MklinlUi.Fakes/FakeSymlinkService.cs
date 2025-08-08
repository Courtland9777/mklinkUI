using MklinlUi.Core;

namespace MklinlUi.Fakes;

/// <summary>
/// Fake implementation of <see cref="ISymlinkService"/> that records created links.
/// </summary>
public class FakeSymlinkService : ISymlinkService
{
    public List<(string linkPath, string targetPath)> Created { get; } = new();

    public Task<SymlinkResult> CreateSymlinkAsync(string linkPath, string targetPath, CancellationToken cancellationToken = default)
    {
        Created.Add((linkPath, targetPath));
        return Task.FromResult(new SymlinkResult(true));
    }
}
