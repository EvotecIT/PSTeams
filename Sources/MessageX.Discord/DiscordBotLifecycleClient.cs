using System.Net.Http;
using System.Net.Http.Headers;

namespace MessageX.Discord;

/// <summary>Updates, deletes, and reacts to Discord messages owned by an authenticated bot.</summary>
public sealed class DiscordBotLifecycleClient :
    IMessageLifecycleClient<DiscordMessageRequest, DiscordDeliveryResult>,
    IMessageReader<DiscordRetrievedMessage>,
    IReactionClient<DiscordDeliveryResult>,
    IDisposable {
    private readonly DiscordConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a lifecycle client with default MessageX transport behavior.</summary>
    public DiscordBotLifecycleClient(DiscordConnection connection)
        : this(connection, DiscordHttpClientPool.Shared) {
    }

    /// <summary>Creates a lifecycle client with configured MessageX transport behavior.</summary>
    public DiscordBotLifecycleClient(DiscordConnection connection, MessageHttpTransportOptions options)
        : this(connection, DiscordHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a lifecycle client over a caller-supplied HTTP client.</summary>
    public DiscordBotLifecycleClient(
        DiscordConnection connection,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public Task<DiscordDeliveryResult> UpdateAsync(
        DiscordMessageRequest message,
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = DiscordLifecycleReference.Validate(reference, MessageCapabilities.Update);
        var target = CreateTarget(reference, coordinates.ConversationId);
        var request = CreateAuthorizedRequest(
            new HttpMethod("PATCH"),
            $"channels/{coordinates.ConversationId}/messages/{coordinates.MessageId}");
        request.Content = DiscordHttpContentFactory.CreateUpdate(message, target);
        return ExecuteMessageAsync(request, target, reference, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DiscordDeliveryResult> DeleteAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = DiscordLifecycleReference.Validate(reference, MessageCapabilities.Delete);
        var target = CreateTarget(reference, coordinates.ConversationId);
        await DiscordBotMessageOwnership.VerifyAsync(
            _httpClient,
            _connection,
            coordinates,
            cancellationToken).ConfigureAwait(false);
        var request = CreateAuthorizedRequest(
            HttpMethod.Delete,
            $"channels/{coordinates.ConversationId}/messages/{coordinates.MessageId}");
        return await ExecuteStatusAsync(
            request,
            target,
            reference,
            MessageCapabilities.None,
            "bot message deletion",
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DiscordRetrievedMessage> GetAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = DiscordLifecycleReference.Validate(reference, MessageCapabilities.Read);
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"channels/{coordinates.ConversationId}/messages/{coordinates.MessageId}");
        return await DiscordLifecycleHttp.ExecuteAsync(
            _httpClient,
            request,
            "bot message retrieval",
            (response, body) => DiscordRetrievedMessageParser.Parse(response, body, reference),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<DiscordDeliveryResult> AddReactionAsync(
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken = default) =>
        ChangeReactionAsync(reference, reaction, remove: false, cancellationToken);

    /// <inheritdoc />
    public Task<DiscordDeliveryResult> RemoveReactionAsync(
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken = default) =>
        ChangeReactionAsync(reference, reaction, remove: true, cancellationToken);

    private Task<DiscordDeliveryResult> ChangeReactionAsync(
        MessageReference reference,
        string reaction,
        bool remove,
        CancellationToken cancellationToken) {
        var coordinates = DiscordLifecycleReference.Validate(reference, MessageCapabilities.React);
        var target = CreateTarget(reference, coordinates.ConversationId);
        var encodedReaction = Uri.EscapeDataString(DiscordReaction.Normalize(reaction));
        var method = remove ? HttpMethod.Delete : HttpMethod.Put;
        var request = CreateAuthorizedRequest(
            method,
            $"channels/{coordinates.ConversationId}/messages/{coordinates.MessageId}/reactions/{encodedReaction}/@me");
        return ExecuteStatusAsync(
            request,
            target,
            reference,
            reference.Capabilities,
            remove ? "bot reaction removal" : "bot reaction addition",
            cancellationToken);
    }

    private async Task<DiscordDeliveryResult> ExecuteMessageAsync(
        HttpRequestMessage request,
        DiscordMessageTarget target,
        MessageReference reference,
        CancellationToken cancellationToken) {
        using (request) {
            var result = await DiscordLifecycleHttp.ExecuteAsync(
                _httpClient,
                request,
                "bot message update",
                (response, body) => DiscordHttpResponseSupport.CreateResult(
                    response,
                    body,
                    target,
                    target.DeliveryMethod),
                cancellationToken).ConfigureAwait(false);
            return DiscordHttpResponseSupport.RequireMatchingCoordinates(result, reference);
        }
    }

    private async Task<DiscordDeliveryResult> ExecuteStatusAsync(
        HttpRequestMessage request,
        DiscordMessageTarget target,
        MessageReference reference,
        MessageCapabilities capabilities,
        string operation,
        CancellationToken cancellationToken) {
        using (request) {
            return await DiscordLifecycleHttp.ExecuteAsync(
                _httpClient,
                request,
                operation,
                (response, body) => DiscordHttpResponseSupport.CreateStatusResult(
                    response,
                    body,
                    target,
                    target.DeliveryMethod,
                    reference,
                    capabilities),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string relativeUri) {
        var request = new HttpRequestMessage(method, new Uri(DiscordConnection.DefaultApiBaseUri, relativeUri));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bot", _connection.BotToken);
        return request;
    }

    private static DiscordMessageTarget CreateTarget(MessageReference reference, string conversationId) {
        if (reference.ConversationKind == MessageConversationKind.DirectMessage) {
            return DiscordMessageTarget.ForDirectMessageChannel(conversationId);
        }
        return reference.ConversationKind == MessageConversationKind.Thread || reference.ThreadId is not null
            ? DiscordMessageTarget.ForThread(conversationId, reference.ScopeId)
            : DiscordMessageTarget.ForChannel(conversationId, reference.ScopeId);
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
