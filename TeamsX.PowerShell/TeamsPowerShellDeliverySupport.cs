using System.Management.Automation;
using System.Net;
using System.Net.Http;
using TeamsX;

namespace TeamsX.PowerShell;

internal static class TeamsPowerShellDeliverySupport {
    public static TeamsClientLease CreateClientLease(Uri? proxy) {
        if (proxy is null) {
            return TeamsClientLease.SharedDefault;
        }

        var handler = new HttpClientHandler {
            Proxy = new WebProxy(proxy),
            UseProxy = true
        };
        var httpClient = new HttpClient(handler, disposeHandler: true);
        var sender = new WebhookTeamsMessageSender(httpClient, disposeHttpClient: true);

        return new TeamsClientLease(
            new TeamsClient(new ITeamsMessageSender[] { sender }),
            sender);
    }

    public static void WriteDeliveryIssue(PSCmdlet cmdlet, TeamsDeliveryResult result, string commandName) {
        if (!result.IsSuccessStatusCode) {
            cmdlet.WriteError(CreateDeliveryFailureError(result, commandName));
            return;
        }

        if (LooksLikeFailureMessage(result.ResponseBody)) {
            var message = $"{commandName} - Couldn't send message. Execute message: {result.ResponseBody}";
            cmdlet.WriteError(new ErrorRecord(
                new InvalidOperationException(message),
                "TeamsMessageDeliveryFailed",
                ErrorCategory.ConnectionError,
                result.TargetUri));
        }
    }

    public static ErrorRecord CreateDeliveryFailureError(TeamsDeliveryResult result, string commandName) {
        var statusCode = result.StatusCode?.ToString() ?? "unknown";
        var message = $"{commandName} - Couldn't send message. HTTP status: {statusCode}.";
        var error = new ErrorRecord(
            new InvalidOperationException(message),
            "TeamsMessageDeliveryFailed",
            ErrorCategory.ConnectionError,
            result.TargetUri) {
            ErrorDetails = new ErrorDetails(result.ResponseBody ?? message)
        };

        return error;
    }

    public static bool LooksLikeFailureMessage(string? responseBody) {
        if (string.IsNullOrWhiteSpace(responseBody)) {
            return false;
        }

        var body = responseBody!;
        return body.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
               body.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

internal sealed class TeamsClientLease : IDisposable {
    public static TeamsClientLease SharedDefault { get; } = new(TeamsClient.Default);

    private readonly IDisposable[] _disposables;

    public TeamsClientLease(TeamsClient client, params IDisposable[] disposables) {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        _disposables = disposables ?? Array.Empty<IDisposable>();
    }

    public TeamsClient Client { get; }

    public void Dispose() {
        foreach (var disposable in _disposables) {
            disposable.Dispose();
        }
    }
}
