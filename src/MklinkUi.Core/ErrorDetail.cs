namespace MklinkUi.Core;

/// <summary>
/// Represents a structured error returned to callers.
/// </summary>
/// <param name="Code">Stable error identifier.</param>
/// <param name="Message">User-safe message.</param>
/// <param name="Details">Optional details without PII.</param>
/// <param name="CorrelationId">Correlation identifier for diagnostics.</param>
public record ErrorDetail(string Code, string Message, string? Details = null, string? CorrelationId = null);
