namespace MklinkUi.Core;

/// <summary>
/// Represents the result of a symbolic link creation attempt.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="ErrorMessage">An optional error message.</param>
/// <param name="ErrorCode">Optional error code.</param>
/// <param name="LinkPath">The created link path when successful.</param>
public record SymlinkResult(bool Success, string? ErrorMessage = null, string? ErrorCode = null, string? LinkPath = null);
