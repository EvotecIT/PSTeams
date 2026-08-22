using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MessageX.Slack;

/// <summary>Updates, deletes, and reacts to application-owned Slack messages.</summary>
public sealed class SlackWebApiLifecycleClient :
    IMessageLifecycleClient<SlackMessageRequest, SlackDeliveryResult>,
    IReactionClient<SlackDeliveryResult>,
    IDisposable {
    private readonly SlackConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a lifecycle client with default MessageX transport behavior.</summary>
    public SlackWebApiLifecycleClient(SlackConnection connection)
        : this(connection, MessageHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a lifecycle client with configured MessageX transport behavior.</summary>
    public SlackWebApiLifecycleClient(SlackConnection connection, MessageHttpTransportOptions options)
        : this(connection, MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a lifecycle client over a caller-supplied HTTP client.</summary>
    public SlackWebApiLifecycleClient(
        SlackConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public Task<SlackDeliveryResult> UpdateAsync(
        SlackMessageRequest message,
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = ValidateReference(reference, MessageCapabilities.Update);
        var json = SlackMessageRenderer.RenderUpdate(message, coordinates.ConversationId, coordinates.Timestamp);
        return ExecuteAsync(
            "chat.update",
            json,
            reference,
            requireReturnedCoordinates: true,
            successCapabilities: ManagedMessageCapabilities,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SlackDeliveryResult> DeleteAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = ValidateReference(reference, MessageCapabilities.Delete);
        var json = JsonSerializer.Serialize(new Dictionary<string, string> {
            ["channel"] = coordinates.ConversationId,
            ["ts"] = coordinates.Timestamp
        });
        return ExecuteAsync(
            "chat.delete",
            json,
            reference,
            requireReturnedCoordinates: false,
            successCapabilities: MessageCapabilities.None,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SlackDeliveryResult> AddReactionAsync(
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken = default) {
        return ChangeReactionAsync("reactions.add", reference, reaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SlackDeliveryResult> RemoveReactionAsync(
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken = default) {
        return ChangeReactionAsync("reactions.remove", reference, reaction, cancellationToken);
    }

    private Task<SlackDeliveryResult> ChangeReactionAsync(
        string method,
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken) {
        var coordinates = ValidateReference(reference, MessageCapabilities.React);
        var normalizedReaction = ValidateReaction(reaction);
        var json = JsonSerializer.Serialize(new Dictionary<string, string> {
            ["channel"] = coordinates.ConversationId,
            ["timestamp"] = coordinates.Timestamp,
            ["name"] = normalizedReaction
        });
        return ExecuteAsync(
            method,
            json,
            reference,
            requireReturnedCoordinates: false,
            successCapabilities: ManagedMessageCapabilities,
            cancellationToken);
    }

    private async Task<SlackDeliveryResult> ExecuteAsync(
        string method,
        string json,
        MessageReference reference,
        bool requireReturnedCoordinates,
        MessageCapabilities successCapabilities,
        CancellationToken cancellationToken) {
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(
            _httpClient,
            cancellationToken);
        try {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(_connection.ApiBaseUri, method)) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _connection.BotToken);
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationCancellation.Token)
                .ConfigureAwait(false);
            var responseBody = await MessageHttpResponseReader
                .ReadUtf8BodyAsync(response, operationCancellation.Token)
                .ConfigureAwait(false);
            return CreateResult(
                method,
                response,
                responseBody,
                reference,
                requireReturnedCoordinates,
                successCapabilities);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException($"Slack Web API {method} request timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException($"Slack Web API {method} request failed.", MessageErrorKind.Transient);
        }
    }

    private SlackDeliveryResult CreateResult(
        string method,
        HttpResponseMessage response,
        string responseBody,
        MessageReference reference,
        bool requireReturnedCoordinates,
        MessageCapabilities successCapabilities) {
        var parsed = SlackApiResponse.Parse(responseBody);
        var returnedTimestamp = SlackMessageValidator.ParseTimestamp(parsed.Timestamp);
        var hasReturnedChannel = SlackMessageTarget.TryNormalizeProviderIdentifier(
            parsed.Channel,
            out var returnedChannel);
        var accepted = response.IsSuccessStatusCode && parsed.IsValid && parsed.Ok &&
            (!requireReturnedCoordinates || (hasReturnedChannel && returnedTimestamp is not null));
        var invalidSuccessEnvelope = parsed.IsValid && parsed.Ok && !accepted;
        var statusCode = (int)response.StatusCode;
        var result = new SlackDeliveryResult {
            DeliveryMethod = SlackDeliveryMethod.WebApi,
            Target = reference.ConversationId ?? "Slack conversation",
            IsSuccess = accepted,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ProviderCode = !parsed.IsValid || invalidSuccessEnvelope ? "invalid_response" : parsed.Error,
            CorrelationId = SlackHttpResponseSupport.ReadCorrelationId(response),
            RetryAfter = SlackHttpResponseSupport.ReadRetryAfter(response)
        };

        if (accepted) {
            result.Reference = CloneReference(
                reference,
                requireReturnedCoordinates ? returnedChannel : reference.ConversationId!,
                requireReturnedCoordinates ? parsed.Timestamp! : reference.MessageId!,
                requireReturnedCoordinates ? returnedTimestamp : reference.Timestamp,
                successCapabilities,
                result.CorrelationId);
            return result;
        }

        result.ErrorKind = invalidSuccessEnvelope || !parsed.IsValid
            ? MessageErrorKind.Transient
            : SlackHttpResponseSupport.Classify(statusCode, result.ProviderCode);
        result.ErrorMessage = result.ProviderCode is null
            ? $"Slack Web API {method} returned HTTP status {statusCode}."
            : $"Slack Web API rejected {method} with '{result.ProviderCode}'.";
        return result;
    }

    private static SlackReferenceCoordinates ValidateReference(
        MessageReference reference,
        MessageCapabilities requiredCapability) {
        if (reference is null) {
            throw new ArgumentNullException(nameof(reference));
        }
        if (!string.Equals(reference.Provider, MessageProviders.Slack, StringComparison.Ordinal)) {
            throw new ArgumentException("A Slack message reference is required.", nameof(reference));
        }
        if ((reference.Capabilities & requiredCapability) != requiredCapability) {
            throw new InvalidOperationException(
                $"The Slack message reference does not support '{requiredCapability}'.");
        }

        var conversationId = SlackMessageTarget.ValidateConversationId(reference.ConversationId);
        if (SlackMessageValidator.ParseTimestamp(reference.MessageId) is null) {
            throw new ArgumentException("Slack message references require a valid message timestamp.", nameof(reference));
        }
        return new SlackReferenceCoordinates(conversationId, reference.MessageId!);
    }

    private static string ValidateReaction(string reaction) {
        var normalized = reaction?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized!.Length > 100 ||
            normalized.Any(character => !(
                character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' or '+'))) {
            throw new ArgumentException(
                "Slack reaction names must contain 1 to 100 letters, digits, underscores, hyphens, or plus signs without colons.",
                nameof(reaction));
        }
        return normalized;
    }

    private static MessageReference CloneReference(
        MessageReference source,
        string conversationId,
        string messageId,
        DateTimeOffset? timestamp,
        MessageCapabilities capabilities,
        string? correlationId) {
        return new MessageReference(MessageProviders.Slack, messageId) {
            InstallationId = source.InstallationId,
            ScopeId = source.ScopeId,
            ConversationId = conversationId,
            ThreadId = source.ThreadId,
            Timestamp = timestamp,
            CorrelationId = correlationId ?? source.CorrelationId,
            Capabilities = capabilities
        };
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    private const MessageCapabilities ManagedMessageCapabilities = MessageCapabilities.Reply |
        MessageCapabilities.Update |
        MessageCapabilities.Delete |
        MessageCapabilities.React;

    private sealed class SlackReferenceCoordinates {
        public SlackReferenceCoordinates(string conversationId, string timestamp) {
            ConversationId = conversationId;
            Timestamp = timestamp;
        }

        public string ConversationId { get; }

        public string Timestamp { get; }
    }
}
