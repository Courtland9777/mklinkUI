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
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        if (Path.IsPathFullyQualified(path))
            return true;
        var first = path[0];
        return first == Path.DirectorySeparatorChar || first == Path.AltDirectorySeparatorChar;
    }

    /// <summary>
    /// Ensures the provided path is absolute and returns the normalized value.
    /// </summary>
    public static string EnsureAbsolute(string path, string paramName)
    {
        if (!IsAbsolutePath(path))
            throw new ArgumentException("Paths must be absolute.", paramName);
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception)
        {
            throw new ArgumentException("Paths must be absolute.", paramName);
        }
    }
}
