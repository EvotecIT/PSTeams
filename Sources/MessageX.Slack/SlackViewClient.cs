using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MessageX.Slack;

/// <summary>Opens Slack modal views from verified short-lived interaction triggers.</summary>
public sealed class SlackViewClient : IDisposable {
    private readonly SlackConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a client with default MessageX transport behavior.</summary>
    public SlackViewClient(SlackConnection connection)
        : this(connection, SlackHttpClientPool.Shared) {
    }

    /// <summary>Creates a client with configured MessageX transport behavior.</summary>
    public SlackViewClient(SlackConnection connection, MessageHttpTransportOptions options)
        : this(connection, MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a client over a caller-supplied HTTP client.</summary>
    public SlackViewClient(
        SlackConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>Opens a modal using a verified interaction trigger.</summary>
    public async Task<SlackViewResult> OpenModalAsync(
        SlackTransientInteractionContext transientContext,
        SlackModalView view,
        CancellationToken cancellationToken = default) {
        if (transientContext is null) {
            throw new ArgumentNullException(nameof(transientContext));
        }
        if (!transientContext.CanOpenModal) {
            throw new InvalidOperationException(
                "The verified Slack interaction modal trigger is unavailable or expired.");
        }
        var json = SlackModalRenderer.RenderOpen(transientContext.TriggerId!, view);
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(_connection.ApiBaseUri, "views.open")) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _connection.BotToken);
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationCancellation.Token)
                .ConfigureAwait(false);
            var responseBody = await MessageHttpResponseReader
                .ReadUtf8BodyAsync(response, operationCancellation.Token)
                .ConfigureAwait(false);
            var parsed = SlackApiResponse.Parse(responseBody);
            var accepted = response.IsSuccessStatusCode && parsed.IsValid && parsed.Ok &&
                IsViewIdentifier(parsed.ViewId);
            var code = !parsed.IsValid || parsed.Ok && !accepted ? "invalid_response" : parsed.Error;
            var result = new SlackViewResult {
                IsSuccess = accepted,
                StatusCode = (int)response.StatusCode,
                ViewId = accepted ? parsed.ViewId : null,
                ProviderCode = accepted ? null : code,
                CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response),
                RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
            };
            if (!accepted) {
                result.ErrorKind = !response.IsSuccessStatusCode || parsed.IsValid && !parsed.Ok
                    ? SlackHttpResponseSupport.Classify((int)response.StatusCode, code)
                    : MessageErrorKind.Transient;
                result.ErrorMessage = code is null
                    ? $"Slack views.open returned HTTP status {(int)response.StatusCode}."
                    : $"Slack Web API rejected views.open with '{code}'.";
            }
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Slack views.open timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException("Slack views.open failed.", MessageErrorKind.Transient);
        }
    }

    private static bool IsViewIdentifier(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value!.Length <= 255 && value[0] == 'V' &&
        value.All(static character => char.IsLetterOrDigit(character));

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
