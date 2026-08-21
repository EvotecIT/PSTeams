using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MessageX.Slack;

/// <summary>Sends authenticated Slack messages through <c>chat.postMessage</c>.</summary>
public sealed class SlackWebApiMessageSender : ISlackMessageSender, IDisposable {
    private readonly SlackConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a sender with default MessageX transport behavior.</summary>
    public SlackWebApiMessageSender(SlackConnection connection)
        : this(connection, MessageHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender with configured MessageX transport behavior.</summary>
    public SlackWebApiMessageSender(SlackConnection connection, MessageHttpTransportOptions options)
        : this(connection, MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender over a caller-supplied HTTP client.</summary>
    public SlackWebApiMessageSender(
        SlackConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public bool CanSend(SlackDeliveryMethod deliveryMethod) => deliveryMethod == SlackDeliveryMethod.WebApi;

    /// <inheritdoc />
    public async Task<SlackDeliveryResult> SendAsync(
        SlackMessageRequest message,
        SlackMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Slack Web API sender cannot use '{target.DeliveryMethod}'.");
        }

        SlackMessageTarget.ValidateConversationId(target.ConversationId);
        var json = SlackMessageRenderer.Render(message, target);
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            return await SendCoreAsync(message, target, json, operationCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Slack Web API request timed out.", MessageErrorKind.Transient);
        }
        catch (HttpRequestException) {
            throw new MessageDeliveryException("Slack Web API request failed.", MessageErrorKind.Transient);
        }
    }

    private async Task<SlackDeliveryResult> SendCoreAsync(
        SlackMessageRequest message,
        SlackMessageTarget target,
        string json,
        CancellationToken cancellationToken) {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(_connection.ApiBaseUri, "chat.postMessage")) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _connection.BotToken);

        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader
            .ReadUtf8BodyAsync(response, cancellationToken)
            .ConfigureAwait(false);
        var parsed = ParseResponse(responseBody);
        var parsedTimestamp = SlackMessageValidator.ParseTimestamp(parsed.Timestamp);
        var hasValidChannel = SlackMessageTarget.TryNormalizeConversationId(
            parsed.Channel,
            out var normalizedChannel);
        var statusCode = (int)response.StatusCode;
        var accepted = response.IsSuccessStatusCode && parsed.IsValid && parsed.Ok &&
            hasValidChannel && parsedTimestamp is not null;
        var invalidSuccessEnvelope = parsed.IsValid && parsed.Ok && !accepted;
        var result = new SlackDeliveryResult {
            DeliveryMethod = SlackDeliveryMethod.WebApi,
            Target = target.SafeLabel(),
            IsSuccess = accepted,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ProviderCode = !parsed.IsValid || invalidSuccessEnvelope ? "invalid_response" : parsed.Error,
            CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response),
            RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
        };

        if (accepted) {
            result.Reference = new MessageReference(MessageProviders.Slack, parsed.Timestamp) {
                ScopeId = _connection.WorkspaceId,
                ConversationId = normalizedChannel,
                ThreadId = message.ThreadTimestamp,
                Timestamp = parsedTimestamp,
                CorrelationId = result.CorrelationId,
                Capabilities = MessageCapabilities.Reply
            };
            return result;
        }

        result.ErrorKind = invalidSuccessEnvelope
            ? MessageErrorKind.Transient
            : !response.IsSuccessStatusCode
            ? SlackHttpResponseSupport.Classify(statusCode, result.ProviderCode)
            : parsed.IsValid
                ? SlackHttpResponseSupport.Classify(statusCode, result.ProviderCode)
                : MessageErrorKind.Transient;
        result.ErrorMessage = result.ProviderCode is null
            ? $"Slack Web API returned HTTP status {statusCode}."
            : $"Slack Web API rejected chat.postMessage with '{result.ProviderCode}'.";
        return result;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    private static ParsedResponse ParseResponse(string responseBody) {
        try {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("ok", out var okElement) ||
                okElement.ValueKind is not JsonValueKind.True and not JsonValueKind.False) {
                return ParsedResponse.Invalid;
            }

            return new ParsedResponse {
                IsValid = true,
                Ok = okElement.GetBoolean(),
                Error = ReadString(root, "error"),
                Channel = ReadString(root, "channel"),
                Timestamp = ReadString(root, "ts")
            };
        }
        catch (JsonException) {
            return ParsedResponse.Invalid;
        }
    }

    private static string? ReadString(JsonElement root, string propertyName) {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private sealed class ParsedResponse {
        public static ParsedResponse Invalid { get; } = new();
        public bool IsValid { get; set; }
        public bool Ok { get; set; }
        public string? Error { get; set; }
        public string? Channel { get; set; }
        public string? Timestamp { get; set; }
    }
}
