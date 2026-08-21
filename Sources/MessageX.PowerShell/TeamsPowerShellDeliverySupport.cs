using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

internal static class TeamsPowerShellDeliverySupport {
    public static TeamsClientLease CreateClientLease(Uri? proxy, int timeoutSeconds = 100, string? userAgent = null) {
        var options = new MessageHttpTransportOptions {
            ProxyUri = proxy,
            Timeout = TimeSpan.FromSeconds(timeoutSeconds),
            UserAgent = userAgent
        };
        var sender = new WebhookTeamsMessageSender(options);

        return new TeamsClientLease(
            new TeamsClient(new ITeamsMessageSender[] { sender }),
            sender);
    }

    public static ErrorRecord CreateDeliveryFailureError(TeamsDeliveryResult result, string commandName) {
        var statusCode = result.StatusCode?.ToString() ?? "unknown";
        var message = string.IsNullOrWhiteSpace(result.ErrorMessage)
            ? $"{commandName} could not send the message. HTTP status: {statusCode}."
            : result.ErrorMessage!;
        var details = $"{message} Error kind: {result.ErrorKind}.";
        if (!string.IsNullOrWhiteSpace(result.CorrelationId)) {
            details += $" Correlation ID: {result.CorrelationId}.";
        }
        if (result.RetryAfter is not null) {
            details += $" Retry after: {Math.Ceiling(result.RetryAfter.Value.TotalSeconds)} seconds.";
        }

        var error = new ErrorRecord(
            new MessageDeliveryException(message, result.ErrorKind, result.StatusCode, result.ProviderCode),
            "TeamsMessageDeliveryFailed",
            ErrorCategory.ConnectionError,
            result.Target) {
            ErrorDetails = new ErrorDetails(details)
        };

        return error;
    }

}

internal sealed class TeamsClientLease : IDisposable {
    private readonly IDisposable[] _disposables;
    private bool _disposed;

    public TeamsClientLease(TeamsClient client, params IDisposable[] disposables) {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        _disposables = disposables ?? Array.Empty<IDisposable>();
    }

    public TeamsClient Client { get; }

    public void Dispose() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        foreach (var disposable in _disposables) {
            disposable.Dispose();
        }
    }
}
