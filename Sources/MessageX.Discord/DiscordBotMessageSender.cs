using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MessageX.Discord;

/// <summary>Sends authenticated Discord bot messages to channels, threads, and one-to-one DMs.</summary>
public sealed class DiscordBotMessageSender : IDiscordMessageSender, IDisposable {
    private readonly DiscordConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a sender with default MessageX transport behavior.</summary>
    public DiscordBotMessageSender(DiscordConnection connection)
        : this(connection, MessageHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender with configured MessageX transport behavior.</summary>
    public DiscordBotMessageSender(DiscordConnection connection, MessageHttpTransportOptions options)
        : this(connection, MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender over a caller-supplied HTTP client.</summary>
    public DiscordBotMessageSender(
        DiscordConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public bool CanSend(DiscordDeliveryMethod deliveryMethod) => deliveryMethod is
        DiscordDeliveryMethod.BotChannel or DiscordDeliveryMethod.BotThread or DiscordDeliveryMethod.BotDirectMessage;

    /// <inheritdoc />
    public async Task<DiscordDeliveryResult> SendAsync(
        DiscordMessageRequest message,
        DiscordMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Discord bot sender cannot use '{target.DeliveryMethod}'.");
        }
        DiscordMessageValidator.Validate(message, target);

        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(_httpClient, cancellationToken);
        try {
            var channelId = target.ChannelId;
            if (target.DeliveryMethod == DiscordDeliveryMethod.BotDirectMessage) {
                var dmResult = await OpenDirectMessageAsync(target, operationCancellation.Token).ConfigureAwait(false);
                if (!dmResult.IsSuccess || dmResult.Reference?.ConversationId is null) {
                    return dmResult;
                }
                channelId = dmResult.Reference.ConversationId;
            }
            return await CreateMessageAsync(message, target, channelId!, operationCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Discord bot request timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException("Discord bot request failed.", MessageErrorKind.Transient);
        }
    }

    private async Task<DiscordDeliveryResult> OpenDirectMessageAsync(
        DiscordMessageTarget target,
        CancellationToken cancellationToken) {
        var payload = JsonSerializer.Serialize(new Dictionary<string, string> {
            ["recipient_id"] = target.UserId!
        });
        using var request = CreateAuthorizedRequest(HttpMethod.Post, "users/@me/channels");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader
            .ReadUtf8BodyAsync(response, cancellationToken)
            .ConfigureAwait(false);
        if (response.IsSuccessStatusCode && TryReadId(responseBody, out var channelId)) {
            return new DiscordDeliveryResult {
                DeliveryMethod = target.DeliveryMethod,
                Target = target.SafeLabel(),
                IsSuccess = true,
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody,
                CorrelationId = DiscordHttpResponseSupport.ReadHeader(response, "cf-ray"),
                RetryAfter = DiscordHttpResponseSupport.ReadRetryAfter(response),
                RateLimitBucket = DiscordHttpResponseSupport.NormalizeDiagnosticToken(
                    DiscordHttpResponseSupport.ReadHeader(response, "x-ratelimit-bucket")),
                RateLimitScope = DiscordHttpResponseSupport.NormalizeDiagnosticToken(
                    DiscordHttpResponseSupport.ReadHeader(response, "x-ratelimit-scope")),
                Reference = new MessageReference(MessageProviders.Discord) {
                    ConversationId = channelId,
                    CorrelationId = DiscordHttpResponseSupport.ReadHeader(response, "cf-ray")
                }
            };
        }

        var failed = DiscordHttpResponseSupport.CreateResult(response, responseBody, target, target.DeliveryMethod);
        if (response.IsSuccessStatusCode) {
            failed.ErrorKind = MessageErrorKind.Transient;
            failed.ProviderCode = "invalid_response";
            failed.ErrorMessage = "Discord returned an invalid direct-message channel response.";
        }
        return failed;
    }

    private async Task<DiscordDeliveryResult> CreateMessageAsync(
        DiscordMessageRequest message,
        DiscordMessageTarget target,
        string channelId,
        CancellationToken cancellationToken) {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"channels/{Uri.EscapeDataString(channelId)}/messages");
        request.Content = DiscordHttpContentFactory.Create(message, target);
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var responseBody = await MessageHttpResponseReader
            .ReadUtf8BodyAsync(response, cancellationToken)
            .ConfigureAwait(false);
        return DiscordHttpResponseSupport.CreateResult(response, responseBody, target, target.DeliveryMethod);
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string relativeUri) {
        var request = new HttpRequestMessage(method, new Uri(DiscordConnection.DefaultApiBaseUri, relativeUri));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bot", _connection.BotToken);
        return request;
    }

    private static bool TryReadId(string responseBody, out string id) {
        try {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("id", out var idElement) &&
                idElement.ValueKind == JsonValueKind.String &&
                DiscordSnowflake.TryNormalize(idElement.GetString(), out var normalized)) {
                id = normalized!;
                return true;
            }
        }
        catch (JsonException) {
        }
        id = string.Empty;
        return false;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
