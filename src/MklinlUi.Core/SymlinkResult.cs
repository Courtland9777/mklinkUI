namespace MklinlUi.Core;

/// <summary>
/// Represents the result of a symbolic link creation attempt.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="ErrorMessage">An optional error message.</param>
public record SymlinkResult(bool Success, string? ErrorMessage = null);
