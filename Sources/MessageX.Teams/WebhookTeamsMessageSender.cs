using System.IO;
using System.Net.Http;
using System.Text;

namespace MessageX.Teams;

public sealed class WebhookTeamsMessageSender : ITeamsMessageSender, ITeamsRawMessageSender, IDisposable {
    internal static WebhookTeamsMessageSender Shared { get; } = new(CreateDefaultHttpClient(), disposeHttpClient: false);

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public WebhookTeamsMessageSender()
        : this(CreateDefaultHttpClient(), disposeHttpClient: true) {
    }

    public WebhookTeamsMessageSender(MessageHttpTransportOptions options)
        : this(CreateDefaultHttpClient(options), disposeHttpClient: true) {
    }

    public WebhookTeamsMessageSender(HttpClient httpClient, bool disposeHttpClient = false) {
        if (httpClient is null) {
            throw new ArgumentNullException(nameof(httpClient));
        }

        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
    }

    public bool CanSend(TeamsDeliveryMethod deliveryMethod) {
        return deliveryMethod is TeamsDeliveryMethod.IncomingWebhook or TeamsDeliveryMethod.WorkflowWebhook;
    }

    public async Task<TeamsDeliveryResult> SendAsync(
        TeamsMessageRequest message,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (message is null) {
            throw new ArgumentNullException(nameof(message));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }

        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Webhook sender cannot send using '{target.DeliveryMethod}'.");
        }

        var json = WebhookMessageRenderer.Render(message);
        return await SendJsonAsync(json, target, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TeamsDeliveryResult> SendJsonAsync(
        string jsonBody,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (jsonBody is null) {
            throw new ArgumentNullException(nameof(jsonBody));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Webhook sender cannot send using '{target.DeliveryMethod}'.");
        }

        TeamsMessageTarget.ValidateUri(target.TargetUri);

        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            return await SendJsonCoreAsync(jsonBody, target, operationCancellation.Token).ConfigureAwait(false);
        } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException(
                "Teams webhook request timed out.",
                MessageErrorKind.Transient);
        } catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException(
                "Teams webhook request failed.",
                MessageErrorKind.Transient);
        }
    }

    private async Task<TeamsDeliveryResult> SendJsonCoreAsync(
        string jsonBody,
        TeamsMessageTarget target,
        CancellationToken cancellationToken) {
        using var request = new HttpRequestMessage(HttpMethod.Post, target.TargetUri) {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader
            .ReadUtf8BodyAsync(response, cancellationToken)
            .ConfigureAwait(false);

        var statusCode = (int)response.StatusCode;
        var isSuccess = response.IsSuccessStatusCode;
        return new TeamsDeliveryResult {
            DeliveryMethod = target.DeliveryMethod,
            Target = string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetUri.Host : target.DisplayName!,
            IsSuccessStatusCode = isSuccess,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            CorrelationId = ReadCorrelationId(response),
            RetryAfter = ReadRetryAfter(response),
            ErrorKind = isSuccess ? MessageErrorKind.Unknown : ClassifyFailure(statusCode),
            ErrorMessage = isSuccess ? null : $"Teams webhook returned HTTP status {statusCode}."
        };
    }

    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    internal static HttpClient CreateDefaultHttpClient(MessageHttpTransportOptions? options = null) {
        return MessageHttpClientFactory.CreateClient(options);
    }

    internal static HttpClientHandler CreateDefaultHandler(MessageHttpTransportOptions? options = null) {
        return MessageHttpClientFactory.CreateHandler(options);
    }

    private static MessageErrorKind ClassifyFailure(int statusCode) {
        return statusCode switch {
            401 => MessageErrorKind.Authentication,
            403 => MessageErrorKind.Authorization,
            404 or 410 => MessageErrorKind.NotFound,
            408 => MessageErrorKind.Transient,
            429 => MessageErrorKind.RateLimited,
            >= 500 => MessageErrorKind.Transient,
            _ => MessageErrorKind.Validation
        };
    }

    private static string? ReadCorrelationId(HttpResponseMessage response) {
        foreach (var name in new[] { "request-id", "x-ms-request-id", "client-request-id" }) {
            if (response.Headers.TryGetValues(name, out var values)) {
                var value = values.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(value)) {
                    return value;
                }
            }
        }

        return null;
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response) {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is not null) {
            return retryAfter.Delta;
        }

        if (retryAfter?.Date is not null) {
            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }

        return null;
    }
}
