using System.IO;
using System.Net.Http;
using System.Text;

namespace MessageX.Slack;

/// <summary>Sends immediate responses using verified, short-lived Slack interaction capabilities.</summary>
public sealed class SlackInteractionResponseClient : IDisposable {
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a client with default MessageX transport behavior.</summary>
    public SlackInteractionResponseClient()
        : this(MessageHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a client with configured MessageX transport behavior.</summary>
    public SlackInteractionResponseClient(MessageHttpTransportOptions options)
        : this(MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a client over a caller-supplied HTTP client.</summary>
    public SlackInteractionResponseClient(HttpClient httpClient, bool disposeHttpClient = false) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>Sends a response while the verified transient capability remains available.</summary>
    public async Task<SlackDeliveryResult> RespondAsync(
        SlackTransientInteractionContext transientContext,
        SlackInteractionResponseRequest response,
        CancellationToken cancellationToken = default) {
        var responseUri = ValidateResponseUri(transientContext);
        var json = SlackMessageRenderer.RenderInteractionResponse(response);
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            using var request = new HttpRequestMessage(HttpMethod.Post, responseUri) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            using var httpResponse = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationCancellation.Token)
                .ConfigureAwait(false);
            await MessageHttpResponseReader
                .ReadUtf8BodyAsync(httpResponse, operationCancellation.Token)
                .ConfigureAwait(false);
            var statusCode = (int)httpResponse.StatusCode;
            var accepted = httpResponse.IsSuccessStatusCode;
            return new SlackDeliveryResult {
                DeliveryMethod = SlackDeliveryMethod.IncomingWebhook,
                Target = "verified Slack interaction response",
                IsSuccess = accepted,
                StatusCode = statusCode,
                ProviderCode = accepted ? null : "interaction_response_failed",
                ErrorKind = accepted
                    ? default
                    : SlackHttpResponseSupport.Classify(statusCode, null),
                ErrorMessage = accepted
                    ? null
                    : $"Slack interaction response returned HTTP status {statusCode}.",
                CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(httpResponse),
                RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(httpResponse)
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Slack interaction response timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException("Slack interaction response failed.", MessageErrorKind.Transient);
        }
    }

    private static Uri ValidateResponseUri(SlackTransientInteractionContext transientContext) {
        if (transientContext is null) {
            throw new ArgumentNullException(nameof(transientContext));
        }
        if (!transientContext.CanRespond ||
            !Uri.TryCreate(transientContext.ResponseUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !uri.IsDefaultPort || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
            (!string.Equals(uri.Host, "hooks.slack.com", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(uri.Host, "hooks.slack-gov.com", StringComparison.OrdinalIgnoreCase)) ||
            (!uri.AbsolutePath.StartsWith("/actions/", StringComparison.Ordinal) &&
             !uri.AbsolutePath.StartsWith("/commands/", StringComparison.Ordinal)) ||
            uri.AbsolutePath.Length > 2048) {
            throw new InvalidOperationException(
                "The verified Slack interaction does not contain a usable response capability.");
        }
        return uri;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
