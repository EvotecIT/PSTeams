using System.Net.Http;
using System.Net;
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

        using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        HttpResponseMessage response;
        try {
            response = await _httpClient.PostAsync(target.TargetUri, content, cancellationToken).ConfigureAwait(false);
        } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException(
                "Teams webhook request timed out.",
                MessageErrorKind.Transient);
        } catch (HttpRequestException) {
            throw new MessageDeliveryException(
                "Teams webhook request failed.",
                MessageErrorKind.Transient);
        }

        using (response) {
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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
    }

    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    internal static HttpClient CreateDefaultHttpClient(MessageHttpTransportOptions? options = null) {
        options ??= new MessageHttpTransportOptions();
        ValidateOptions(options);

        var client = new HttpClient(CreateDefaultHandler(options)) {
            Timeout = options.Timeout
        };
        var userAgent = options.UserAgent?.Trim();
        if (!string.IsNullOrWhiteSpace(userAgent)) {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        return client;
    }

    internal static HttpClientHandler CreateDefaultHandler(MessageHttpTransportOptions? options = null) {
        options ??= new MessageHttpTransportOptions();
        ValidateOptions(options);

        var handler = new HttpClientHandler {
            AllowAutoRedirect = false
        };
        if (options.ProxyUri is not null) {
            handler.Proxy = new WebProxy(options.ProxyUri);
            handler.UseProxy = true;
        }

        return handler;
    }

    private static void ValidateOptions(MessageHttpTransportOptions options) {
        if (options.Timeout <= TimeSpan.Zero && options.Timeout != Timeout.InfiniteTimeSpan) {
            throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be positive or infinite.");
        }

        if (options.ProxyUri is not null &&
            (!options.ProxyUri.IsAbsoluteUri ||
             (options.ProxyUri.Scheme != Uri.UriSchemeHttp && options.ProxyUri.Scheme != Uri.UriSchemeHttps))) {
            throw new ArgumentException("Proxy URI must be an absolute HTTP or HTTPS URI.", nameof(options));
        }
    }

    private static MessageErrorKind ClassifyFailure(int statusCode) {
        return statusCode switch {
            401 => MessageErrorKind.Authentication,
            403 => MessageErrorKind.Authorization,
            404 => MessageErrorKind.NotFound,
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
