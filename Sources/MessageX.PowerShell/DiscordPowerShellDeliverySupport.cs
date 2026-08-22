using System.Management.Automation;

namespace MessageX.PowerShell;

internal static class DiscordPowerShellDeliverySupport {
    public static ErrorRecord CreateDeliveryFailureError(DiscordDeliveryResult result, string commandName) {
        var statusCode = result.StatusCode?.ToString() ?? "unknown";
        var message = string.IsNullOrWhiteSpace(result.ErrorMessage)
            ? $"{commandName} could not send the message. HTTP status: {statusCode}."
            : result.ErrorMessage!;
        var details = $"{message} Error kind: {result.ErrorKind}.";
        if (!string.IsNullOrWhiteSpace(result.CorrelationId)) {
            details += $" Correlation ID: {result.CorrelationId}.";
        }
        if (!string.IsNullOrWhiteSpace(result.RateLimitBucket)) {
            details += $" Rate-limit bucket: {result.RateLimitBucket}.";
        }
        if (result.RetryAfter is not null) {
            details += $" Retry after: {Math.Ceiling(result.RetryAfter.Value.TotalSeconds)} seconds.";
        }

        return new ErrorRecord(
            new MessageDeliveryException(message, result.ErrorKind, result.StatusCode, result.ProviderCode),
            "DiscordMessageDeliveryFailed",
            ErrorCategory.ConnectionError,
            result.Target) {
            ErrorDetails = new ErrorDetails(details)
        };
    }
}
