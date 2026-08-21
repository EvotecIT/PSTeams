using System.Net.Http;
using System.Text;

namespace MessageX.Teams;

public sealed class WebhookTeamsMessageSender : ITeamsMessageSender, ITeamsRawMessageSender, IDisposable {
    internal static WebhookTeamsMessageSender Shared { get; } = new(CreateDefaultHttpClient(), disposeHttpClient: false);

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public WebhookTeamsMessageSender()
        : this(CreateDefaultHttpClient(), disposeHttpClient: true) {
    }

    public WebhookTeamsMessageSender(HttpClient httpClient, bool disposeHttpClient = false) {
        if (httpClient is null) {
            throw new ArgumentNullException(nameof(httpClient));
        }

        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
    }

    public bool CanSend(TeamsDeliveryMethod deliveryMethod) {
        return deliveryMethod is TeamsDeliveryMethod.IncomingWebhook or TeamsDeliveryMethod.WorkflowWebhook;
    }

    public async Task<TeamsDeliveryResult> SendAsync(
        TeamsMessageRequest message,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (message is null) {
            throw new ArgumentNullException(nameof(message));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }

        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Webhook sender cannot send using '{target.DeliveryMethod}'.");
        }

        var json = WebhookMessageRenderer.Render(message);
        return await SendJsonAsync(json, target, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TeamsDeliveryResult> SendJsonAsync(
        string jsonBody,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (jsonBody is null) {
            throw new ArgumentNullException(nameof(jsonBody));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (!CanSend(target.DeliveryMethod)) {
            throw new InvalidOperationException($"Webhook sender cannot send using '{target.DeliveryMethod}'.");
        }

        TeamsMessageTarget.ValidateUri(target.TargetUri);

        using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(target.TargetUri, content, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return new TeamsDeliveryResult {
            DeliveryMethod = target.DeliveryMethod,
            Target = string.IsNullOrWhiteSpace(target.DisplayName) ? target.TargetUri.Host : target.DisplayName!,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            ResponseBody = responseBody
        };
    }

    public void Dispose() {
        if (_disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    internal static HttpClient CreateDefaultHttpClient() {
        return new HttpClient(CreateDefaultHandler());
    }

    internal static HttpClientHandler CreateDefaultHandler() {
        return new HttpClientHandler {
            AllowAutoRedirect = false
        };
    }
}
