namespace MklinkUi.Core;

/// <summary>
/// Utility helpers for path validation.
/// </summary>
public static class PathHelpers
{
    /// <summary>
    /// Returns <c>true</c> when all provided paths are non-empty and fully qualified.
    /// </summary>
    public static bool AreFullyQualified(params string?[] paths)
    {
        foreach (var path in paths)
        {
            if (!IsFullyQualified(path))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns <c>true</c> when the given path is non-empty and fully qualified.
    /// </summary>
    public static bool IsFullyQualified(string? path) =>
        !string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path);
}
