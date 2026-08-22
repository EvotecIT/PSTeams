using System.Net.Http;
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
    private readonly SlackWebApiInvoker _invoker;

    /// <summary>Creates a lifecycle client with default MessageX transport behavior.</summary>
    public SlackWebApiLifecycleClient(SlackConnection connection)
        : this(connection, SlackHttpClientPool.Shared) {
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
        _invoker = new SlackWebApiInvoker(_connection, _httpClient);
    }

    /// <inheritdoc />
    public Task<SlackDeliveryResult> UpdateAsync(
        SlackMessageRequest message,
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = ValidateReference(reference, MessageCapabilities.Update);
        var json = SlackMessageRenderer.RenderUpdate(message, coordinates.ConversationId, coordinates.Timestamp);
        return _invoker.ExecuteAsync(
            "chat.update",
            json,
            reference.ConversationId!,
            parsed => SlackMessageTarget.TryNormalizeProviderIdentifier(parsed.Channel, out _) &&
                SlackMessageValidator.ParseTimestamp(parsed.Timestamp) is not null,
            (parsed, correlationId) => CloneReference(
                reference,
                parsed.Channel!,
                parsed.Timestamp!,
                SlackMessageValidator.ParseTimestamp(parsed.Timestamp),
                reference.Capabilities,
                correlationId),
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
        return _invoker.ExecuteAsync(
            "chat.delete",
            json,
            coordinates.ConversationId,
            _ => true,
            (_, correlationId) => CloneReference(
                reference,
                coordinates.ConversationId,
                coordinates.Timestamp,
                reference.Timestamp,
                MessageCapabilities.None,
                correlationId),
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
        return _invoker.ExecuteAsync(
            method,
            json,
            coordinates.ConversationId,
            _ => true,
            (_, correlationId) => CloneReference(
                reference,
                coordinates.ConversationId,
                coordinates.Timestamp,
                reference.Timestamp,
                reference.Capabilities,
                correlationId),
            cancellationToken);
    }

    private SlackReferenceCoordinates ValidateReference(
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
        var referenceScope = reference.ScopeId?.Trim();
        if (!string.IsNullOrWhiteSpace(referenceScope) &&
            !string.IsNullOrWhiteSpace(_connection.WorkspaceId) &&
            !string.Equals(referenceScope, _connection.WorkspaceId, StringComparison.Ordinal)) {
            throw new ArgumentException(
                "The Slack message reference belongs to a different workspace.",
                nameof(reference));
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

    private sealed class SlackReferenceCoordinates {
        public SlackReferenceCoordinates(string conversationId, string timestamp) {
            ConversationId = conversationId;
            Timestamp = timestamp;
        }

        public string ConversationId { get; }

        public string Timestamp { get; }
    }
}
