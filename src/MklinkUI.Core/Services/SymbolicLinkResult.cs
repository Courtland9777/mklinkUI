namespace MklinkUI.Core.Services;

/// <summary>
/// Result of attempting to create a symbolic link.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="ErrorMessage">Optional error message if the operation failed.</param>
/// <param name="ErrorCode">Optional native error code when a failure occurs.</param>
public record SymbolicLinkResult(bool Success, string? ErrorMessage, int ErrorCode = 0);

