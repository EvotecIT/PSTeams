using System.IO;
using System.Net.Http;

namespace MessageX.Discord;

/// <summary>Sends messages through Discord incoming webhooks.</summary>
public sealed class DiscordIncomingWebhookSender : IDiscordMessageSender, IDisposable {
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a sender with default MessageX transport behavior.</summary>
    public DiscordIncomingWebhookSender()
        : this(DiscordHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender with configured MessageX transport behavior.</summary>
    public DiscordIncomingWebhookSender(MessageHttpTransportOptions options)
        : this(DiscordHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a sender over a caller-supplied HTTP client.</summary>
    public DiscordIncomingWebhookSender(HttpClient httpClient, bool disposeHttpClient = false) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <inheritdoc />
    public bool CanSend(DiscordDeliveryMethod deliveryMethod) =>
        deliveryMethod == DiscordDeliveryMethod.IncomingWebhook;

    /// <inheritdoc />
    public async Task<DiscordDeliveryResult> SendAsync(
        DiscordMessageRequest message,
        DiscordMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Discord incoming-webhook sender cannot use '{target.DeliveryMethod}'.");
        }

        DiscordMessageTarget.ValidateWebhookUri(target.WebhookUri);
        DiscordMessageValidator.Validate(message, target);
        using var operationCancellation = MessageHttpClientFactory.CreateOperationCancellation(_httpClient, cancellationToken);
        try {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                CreateExecutionUri(target, message.Components.Count > 0)) {
                Content = DiscordHttpContentFactory.Create(message, target)
            };
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationCancellation.Token)
                .ConfigureAwait(false);
            var responseBody = await MessageHttpResponseReader
                .ReadUtf8BodyAsync(response, operationCancellation.Token)
                .ConfigureAwait(false);
            return DiscordHttpResponseSupport.CreateResult(response, responseBody, target, target.DeliveryMethod);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            throw new MessageDeliveryException("Discord incoming-webhook request timed out.", MessageErrorKind.Transient);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new MessageDeliveryException("Discord incoming-webhook request failed.", MessageErrorKind.Transient);
        }
    }

    internal static Uri CreateExecutionUri(DiscordMessageTarget target, bool withComponents = false) {
        var builder = new UriBuilder(target.WebhookUri!);
        var query = "wait=true";
        if (target.ThreadId is not null) {
            query += "&thread_id=" + Uri.EscapeDataString(target.ThreadId);
        }
        if (withComponents) {
            if (!target.SupportsInteractiveComponents) {
                throw new ArgumentException(
                    "Discord interactive components require an application-owned webhook target.",
                    nameof(target));
            }
            query += "&with_components=true";
        }
        builder.Query = query;
        return builder.Uri;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
