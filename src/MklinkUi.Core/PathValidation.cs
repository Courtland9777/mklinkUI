using System;
using System.IO;

namespace MklinkUi.Core;

/// <summary>
/// Helpers for validating and normalizing file system paths.
/// </summary>
public static class PathValidation
{
    /// <summary>
    /// Determines whether the provided path resolves to an absolute path.
    /// </summary>
    public static bool IsAbsolutePath(string? path)
        => !string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path);

    /// <summary>
    /// Ensures the provided path is absolute and returns the normalized value.
    /// </summary>
    public static string EnsureAbsolute(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Paths must be absolute.");

        var full = Path.GetFullPath(path);
        if (!Path.IsPathFullyQualified(full))
            throw new ArgumentException("Paths must be absolute.");

        return full;
    }
}
