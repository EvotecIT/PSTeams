using System.Globalization;
using System.Net.Http;

namespace MessageX.Discord;

/// <summary>Manages Discord interaction responses while the short-lived interaction token remains valid.</summary>
public sealed class DiscordInteractionResponseClient : IDisposable {
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>Creates a client with default MessageX transport behavior.</summary>
    public DiscordInteractionResponseClient()
        : this(DiscordHttpClientFactory.CreateClient(), disposeHttpClient: true) {
    }

    /// <summary>Creates a client with configured MessageX transport behavior.</summary>
    public DiscordInteractionResponseClient(MessageHttpTransportOptions options)
        : this(DiscordHttpClientFactory.CreateClient(options), disposeHttpClient: true) {
    }

    /// <summary>Creates a client over a caller-supplied HTTP client.</summary>
    public DiscordInteractionResponseClient(HttpClient httpClient, bool disposeHttpClient = false) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>Creates a follow-up message and returns its durable message reference.</summary>
    public async Task<DiscordDeliveryResult> FollowUpAsync(
        DiscordTransientInteractionContext transientContext,
        DiscordMessageRequest message,
        CancellationToken cancellationToken = default) {
        var target = CreateTarget(transientContext);
        using var sender = new DiscordIncomingWebhookSender(_httpClient);
        return await sender.SendAsync(message, target, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Updates the original interaction response.</summary>
    public async Task<DiscordDeliveryResult> UpdateOriginalAsync(
        DiscordTransientInteractionContext transientContext,
        DiscordMessageRequest message,
        CancellationToken cancellationToken = default) {
        var target = CreateTarget(transientContext);
        using var request = new HttpRequestMessage(
            new HttpMethod("PATCH"),
            CreateOriginalMessageUri(target, message.Components.Count > 0)) {
            Content = DiscordHttpContentFactory.CreateUpdate(message, target)
        };
        return await DiscordLifecycleHttp.ExecuteAsync(
            _httpClient,
            request,
            "interaction original response update",
            (response, body) => DiscordHttpResponseSupport.CreateResult(
                response,
                body,
                target,
                DiscordDeliveryMethod.IncomingWebhook),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes the original interaction response.</summary>
    public async Task<DiscordDeliveryResult> DeleteOriginalAsync(
        DiscordTransientInteractionContext transientContext,
        CancellationToken cancellationToken = default) {
        var target = CreateTarget(transientContext);
        using var request = new HttpRequestMessage(HttpMethod.Delete, CreateOriginalMessageUri(target));
        return await DiscordLifecycleHttp.ExecuteAsync(
            _httpClient,
            request,
            "interaction original response deletion",
            (response, _) => CreateStatusResult(response, target, "delete original interaction response"),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Updates a follow-up message created through this interaction webhook.</summary>
    public async Task<DiscordDeliveryResult> UpdateFollowUpAsync(
        DiscordTransientInteractionContext transientContext,
        DiscordMessageRequest message,
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var target = CreateTarget(transientContext);
        using var lifecycle = new DiscordWebhookLifecycleClient(target, _httpClient);
        return await lifecycle.UpdateAsync(message, reference, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes a follow-up message created through this interaction webhook.</summary>
    public async Task<DiscordDeliveryResult> DeleteFollowUpAsync(
        DiscordTransientInteractionContext transientContext,
        MessageReference reference,
        CancellationToken cancellationToken = default) {
        var target = CreateTarget(transientContext);
        using var lifecycle = new DiscordWebhookLifecycleClient(target, _httpClient);
        return await lifecycle.DeleteAsync(reference, cancellationToken).ConfigureAwait(false);
    }

    private static DiscordMessageTarget CreateTarget(DiscordTransientInteractionContext transientContext) {
        if (transientContext is null) {
            throw new ArgumentNullException(nameof(transientContext));
        }
        if (!transientContext.CanFollowUp || transientContext.Token is null) {
            throw new InvalidOperationException(
                "The Discord interaction follow-up capability is unavailable or expired.");
        }
        var applicationId = DiscordSnowflake.Normalize(
            transientContext.ApplicationId,
            nameof(transientContext));
        if (transientContext.Token.Length < 20 || transientContext.Token.Length > 512 ||
            transientContext.Token.Any(static character => char.IsWhiteSpace(character) || char.IsControl(character))) {
            throw new InvalidOperationException("The Discord interaction follow-up capability is invalid.");
        }

        return DiscordMessageTarget.ForApplicationWebhook(new Uri(
            $"https://discord.com/api/v10/webhooks/{applicationId}/{Uri.EscapeDataString(transientContext.Token)}",
            UriKind.Absolute));
    }

    private static Uri CreateOriginalMessageUri(DiscordMessageTarget target, bool withComponents = false) {
        var builder = new UriBuilder(target.WebhookUri!);
        builder.Path = builder.Path.TrimEnd('/') + "/messages/@original";
        if (withComponents) {
            builder.Query = "with_components=true";
        }
        return builder.Uri;
    }

    private static DiscordDeliveryResult CreateStatusResult(
        HttpResponseMessage response,
        DiscordMessageTarget target,
        string operation) {
        var statusCode = (int)response.StatusCode;
        var accepted = response.IsSuccessStatusCode;
        return new DiscordDeliveryResult {
            DeliveryMethod = DiscordDeliveryMethod.IncomingWebhook,
            Target = target.SafeLabel(),
            IsSuccess = accepted,
            StatusCode = statusCode,
            ProviderCode = accepted ? null : "http_" + statusCode.ToString(CultureInfo.InvariantCulture),
            ErrorKind = accepted ? default : DiscordHttpResponseSupport.Classify(statusCode),
            ErrorMessage = accepted ? null : $"Discord rejected {operation} with HTTP status {statusCode}.",
            CorrelationId = DiscordHttpResponseSupport.ReadHeader(response, "cf-ray"),
            RetryAfter = DiscordHttpResponseSupport.ReadRetryAfter(response),
            RateLimitBucket = DiscordHttpResponseSupport.NormalizeDiagnosticToken(
                DiscordHttpResponseSupport.ReadHeader(response, "x-ratelimit-bucket")),
            RateLimitScope = DiscordHttpResponseSupport.NormalizeDiagnosticToken(
                DiscordHttpResponseSupport.ReadHeader(response, "x-ratelimit-scope"))
        };
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }
}
