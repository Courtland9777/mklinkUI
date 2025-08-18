namespace MklinkUi.Core;

/// <summary>
/// Well-known error codes for user-visible failures.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Provided path is not absolute.</summary>
    public const string InvalidPath = "E_INVALID_PATH";
    /// <summary>Operation requires developer mode or elevation.</summary>
    public const string DevModeRequired = "E_DEV_MODE_REQUIRED";
    /// <summary>Unexpected error.</summary>
    public const string Unexpected = "E_UNEXPECTED";
}
