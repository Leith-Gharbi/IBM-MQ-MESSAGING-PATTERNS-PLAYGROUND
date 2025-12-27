namespace MqPlayground.Api.Models;

/// <summary>
/// Standard error response for API errors.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error type or code.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// When the error occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new error response.
    /// </summary>
    public static ErrorResponse Create(string error, string message)
    {
        return new ErrorResponse
        {
            Error = error,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }
}
