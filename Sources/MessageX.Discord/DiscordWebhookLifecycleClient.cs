using System.Net.Http;

namespace MessageX.Discord;

/// <summary>Retrieves, updates, and deletes messages owned by one Discord incoming webhook.</summary>
public sealed class DiscordWebhookLifecycleClient :
    IMessageLifecycleClient<DiscordMessageRequest, DiscordDeliveryResult>,
    IMessageReader<DiscordRetrievedMessage>,
    IDisposable {
    private readonly DiscordMessageTarget _target;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a lifecycle client with default MessageX transport behavior.</summary>
    public DiscordWebhookLifecycleClient(DiscordMessageTarget target)
        : this(target, DiscordHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a lifecycle client with configured MessageX transport behavior.</summary>
    public DiscordWebhookLifecycleClient(DiscordMessageTarget target, MessageHttpTransportOptions options)
        : this(target, DiscordHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a lifecycle client over a caller-supplied HTTP client.</summary>
    public DiscordWebhookLifecycleClient(
        DiscordMessageTarget target,
        HttpClient httpClient,
        bool disposeHttpClient = false) {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        if (_target.DeliveryMethod != DiscordDeliveryMethod.IncomingWebhook) {
            throw new ArgumentException("A Discord incoming-webhook target is required.", nameof(target));
        }
        DiscordMessageTarget.ValidateWebhookUri(_target.WebhookUri);
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public Task<DiscordDeliveryResult> UpdateAsync(
        DiscordMessageRequest message,
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = ValidateReference(reference, MessageCapabilities.Update);
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), CreateMessageUri(coordinates.MessageId)) {
            Content = DiscordHttpContentFactory.CreateUpdate(message, _target)
        };
        return ExecuteStatusOrMessageAsync(request, reference, expectMessage: true, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DiscordDeliveryResult> DeleteAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = ValidateReference(reference, MessageCapabilities.Delete);
        var request = new HttpRequestMessage(HttpMethod.Delete, CreateMessageUri(coordinates.MessageId));
        return ExecuteStatusOrMessageAsync(request, reference, expectMessage: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DiscordRetrievedMessage> GetAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var coordinates = ValidateReference(reference, MessageCapabilities.Read);
        using var request = new HttpRequestMessage(HttpMethod.Get, CreateMessageUri(coordinates.MessageId));
        return await DiscordLifecycleHttp.ExecuteAsync(
            _httpClient,
            request,
            "webhook message retrieval",
            (response, body) => DiscordRetrievedMessageParser.Parse(response, body, reference),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<DiscordDeliveryResult> ExecuteStatusOrMessageAsync(
        HttpRequestMessage request,
        MessageReference reference,
        bool expectMessage,
        CancellationToken cancellationToken) {
        using (request) {
            var result = await DiscordLifecycleHttp.ExecuteAsync(
                _httpClient,
                request,
                expectMessage ? "webhook message update" : "webhook message deletion",
                (response, body) => expectMessage
                    ? DiscordHttpResponseSupport.CreateResult(
                        response,
                        body,
                        _target,
                        DiscordDeliveryMethod.IncomingWebhook)
                    : DiscordHttpResponseSupport.CreateStatusResult(
                        response,
                        body,
                        _target,
                        DiscordDeliveryMethod.IncomingWebhook,
                        reference,
                        MessageCapabilities.None),
                cancellationToken).ConfigureAwait(false);
            return expectMessage
                ? DiscordHttpResponseSupport.RequireMatchingCoordinates(result, reference)
                : result;
        }
    }

    private DiscordLifecycleReference.Coordinates ValidateReference(
        MessageReference reference,
        MessageCapabilities requiredCapability) {
        var coordinates = DiscordLifecycleReference.Validate(reference, requiredCapability);
        if (!string.Equals(reference.ThreadId, _target.ThreadId, StringComparison.Ordinal)) {
            throw new ArgumentException(
                "The Discord webhook target and message reference have different thread coordinates.",
                nameof(reference));
        }
        return coordinates;
    }

    private Uri CreateMessageUri(string messageId) {
        var builder = new UriBuilder(_target.WebhookUri!);
        builder.Path = builder.Path.TrimEnd('/') + "/messages/" + Uri.EscapeDataString(messageId);
        builder.Query = _target.ThreadId is null
            ? string.Empty
            : "thread_id=" + Uri.EscapeDataString(_target.ThreadId);
        return builder.Uri;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
