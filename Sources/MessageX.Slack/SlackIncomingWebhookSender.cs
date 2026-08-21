using System.Net.Http;
using System.Text;

namespace MessageX.Slack;

/// <summary>Sends messages through Slack incoming webhooks.</summary>
public sealed class SlackIncomingWebhookSender : ISlackMessageSender, IDisposable {
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a sender with default MessageX transport behavior.</summary>
    public SlackIncomingWebhookSender()
        : this(MessageHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender with configured MessageX transport behavior.</summary>
    public SlackIncomingWebhookSender(MessageHttpTransportOptions options)
        : this(MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender over a caller-supplied HTTP client.</summary>
    public SlackIncomingWebhookSender(HttpClient httpClient, bool disposeHttpClient = false) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public bool CanSend(SlackDeliveryMethod deliveryMethod) => deliveryMethod == SlackDeliveryMethod.IncomingWebhook;

    /// <inheritdoc />
    public async Task<SlackDeliveryResult> SendAsync(
        SlackMessageRequest message,
        SlackMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Slack incoming-webhook sender cannot use '{target.DeliveryMethod}'.");
        }

        SlackMessageTarget.ValidateWebhookUri(target.WebhookUri);
        var json = SlackMessageRenderer.Render(message, target);
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            return await SendCoreAsync(json, target, operationCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Slack incoming-webhook request timed out.", MessageErrorKind.Transient);
        }
        catch (HttpRequestException) {
            throw new MessageDeliveryException("Slack incoming-webhook request failed.", MessageErrorKind.Transient);
        }
    }

    private async Task<SlackDeliveryResult> SendCoreAsync(
        string json,
        SlackMessageTarget target,
        CancellationToken cancellationToken) {
        using var request = new HttpRequestMessage(HttpMethod.Post, target.WebhookUri) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader
            .ReadUtf8BodyAsync(response, cancellationToken)
            .ConfigureAwait(false);
        var statusCode = (int)response.StatusCode;
        var result = new SlackDeliveryResult {
            DeliveryMethod = SlackDeliveryMethod.IncomingWebhook,
            Target = target.SafeLabel(),
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ProviderCode = response.IsSuccessStatusCode ? null : responseBody.Trim(),
            CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response),
            RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
        };
        result.ErrorKind = result.IsSuccess
            ? MessageErrorKind.Unknown
            : SlackHttpResponseSupport.Classify(statusCode, result.ProviderCode);
        result.ErrorMessage = result.IsSuccess
            ? null
            : result.ProviderCode is null
                ? $"Slack incoming webhook returned HTTP status {statusCode}."
                : $"Slack incoming webhook rejected the message with '{result.ProviderCode}'.";
        return result;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
