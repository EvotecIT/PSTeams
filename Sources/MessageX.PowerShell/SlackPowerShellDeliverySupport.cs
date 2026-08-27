using System.Management.Automation;

namespace MessageX.PowerShell;

internal static class SlackPowerShellDeliverySupport {
    public static ErrorRecord CreateDeliveryFailureError(SlackDeliveryResult result, string commandName) {
        var statusCode = result.StatusCode?.ToString() ?? "unknown";
        var message = string.IsNullOrWhiteSpace(result.ErrorMessage)
            ? $"{commandName} could not complete the Slack operation. HTTP status: {statusCode}."
            : result.ErrorMessage!;
        var details = $"{message} Error kind: {result.ErrorKind}.";
        if (!string.IsNullOrWhiteSpace(result.CorrelationId)) {
            details += $" Correlation ID: {result.CorrelationId}.";
        }
        if (result.RetryAfter is not null) {
            details += $" Retry after: {Math.Ceiling(result.RetryAfter.Value.TotalSeconds)} seconds.";
        }

        return new ErrorRecord(
            new MessageDeliveryException(message, result.ErrorKind, result.StatusCode, result.ProviderCode),
            "SlackMessageDeliveryFailed",
            ErrorCategory.ConnectionError,
            result.Target) {
            ErrorDetails = new ErrorDetails(details)
        };
    }

    public static ErrorRecord CreateFileUploadFailureError(SlackFileUploadResult result, string commandName) {
        var statusCode = result.StatusCode?.ToString() ?? "unknown";
        var message = string.IsNullOrWhiteSpace(result.ErrorMessage)
            ? $"{commandName} could not upload the Slack file. HTTP status: {statusCode}."
            : result.ErrorMessage!;
        var details = $"{message} Error kind: {result.ErrorKind}.";
        if (!string.IsNullOrWhiteSpace(result.CorrelationId)) {
            details += $" Correlation ID: {result.CorrelationId}.";
        }
        if (result.RetryAfter is not null) {
            details += $" Retry after: {Math.Ceiling(result.RetryAfter.Value.TotalSeconds)} seconds.";
        }

        return new ErrorRecord(
            new MessageDeliveryException(message, result.ErrorKind, result.StatusCode, result.ProviderCode),
            "SlackFileUploadFailed",
            ErrorCategory.ConnectionError,
            result.FileName) {
            ErrorDetails = new ErrorDetails(details)
        };
    }
}
