using System.Net.Http;
using System.Text.Json;

namespace MessageX.Slack;

/// <summary>Opens or resolves explicitly addressed Slack direct-message conversations.</summary>
public sealed class SlackConversationDirectory : IDisposable {
    private readonly SlackConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private readonly SlackWebApiInvoker _invoker;

    /// <summary>Creates a directory with default MessageX transport behavior.</summary>
    public SlackConversationDirectory(SlackConnection connection)
        : this(connection, SlackHttpClientPool.Shared) {
    }

    /// <summary>Creates a directory with configured MessageX transport behavior.</summary>
    public SlackConversationDirectory(SlackConnection connection, MessageHttpTransportOptions options)
        : this(connection, MessageHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a directory over a caller-supplied HTTP client.</summary>
    public SlackConversationDirectory(
        SlackConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
        _invoker = new SlackWebApiInvoker(_connection, _httpClient);
    }

    /// <summary>
    /// Opens or reuses a direct-message conversation for one to eight explicit Slack user identifiers.
    /// </summary>
    /// <param name="userIds">Slack user identifiers; display names are not accepted.</param>
    /// <param name="preventCreation">When true, resolves only an already existing conversation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<SlackDeliveryResult> OpenDirectMessageAsync(
        IEnumerable<string> userIds,
        bool preventCreation = false,
        CancellationToken cancellationToken = default) {
        var normalizedUserIds = ValidateUserIds(userIds);
        var json = JsonSerializer.Serialize(new Dictionary<string, object> {
            ["users"] = string.Join(",", normalizedUserIds),
            ["prevent_creation"] = preventCreation
        });
        var safeTarget = normalizedUserIds.Length == 1
            ? $"Slack direct message to {normalizedUserIds[0]}"
            : $"Slack multiparty direct message to {normalizedUserIds.Length} users";
        return _invoker.ExecuteAsync(
            "conversations.open",
            json,
            safeTarget,
            parsed => SlackMessageTarget.TryNormalizeProviderIdentifier(parsed.Channel, out var channelId) &&
                channelId[0] is 'D' or 'G',
            (parsed, correlationId) => new MessageReference(MessageProviders.Slack) {
                ScopeId = _connection.WorkspaceId,
                ConversationId = parsed.Channel!.Trim(),
                ConversationKind = MessageConversationKind.DirectMessage,
                CorrelationId = correlationId,
                Capabilities = MessageCapabilities.Send | MessageCapabilities.Reply
            },
            cancellationToken);
    }

    private static string[] ValidateUserIds(IEnumerable<string> userIds) {
        if (userIds is null) {
            throw new ArgumentNullException(nameof(userIds));
        }
        var normalized = userIds.Select(ValidateUserId).ToArray();
        if (normalized.Length is < 1 or > 8) {
            throw new ArgumentException(
                "Slack direct-message resolution requires between one and eight explicit user identifiers.",
                nameof(userIds));
        }
        if (normalized.Distinct(StringComparer.Ordinal).Count() != normalized.Length) {
            throw new ArgumentException("Slack direct-message user identifiers must be unique.", nameof(userIds));
        }
        return normalized;
    }

    private static string ValidateUserId(string userId) {
        var normalized = SlackMessageTarget.ValidateConversationId(userId);
        if (normalized[0] is not ('U' or 'W')) {
            throw new ArgumentException(
                "Slack direct-message resolution accepts user identifiers beginning with U or W.",
                nameof(userId));
        }
        return normalized;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
