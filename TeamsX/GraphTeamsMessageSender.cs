using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace TeamsX;

public sealed class GraphTeamsMessageSender : ITeamsMessageSender, ITeamsRawMessageSender, IDisposable {
    internal static GraphTeamsMessageSender Shared { get; } = new(new HttpClient(), disposeHttpClient: false);

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public GraphTeamsMessageSender()
        : this(new HttpClient(), disposeHttpClient: true) {
    }

    public GraphTeamsMessageSender(HttpClient httpClient, bool disposeHttpClient = false) {
        if (httpClient is null) {
            throw new ArgumentNullException(nameof(httpClient));
        }

        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
    }

    public bool CanSend(TeamsDeliveryMethod deliveryMethod) {
        return deliveryMethod is TeamsDeliveryMethod.GraphChannelMessage or TeamsDeliveryMethod.GraphChatMessage;
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
            throw new InvalidOperationException($"Graph sender cannot send using '{target.DeliveryMethod}'.");
        }

        var jsonBody = GraphMessageRenderer.Render(message, target.DeliveryMethod);
        return await SendJsonAsync(jsonBody, target, cancellationToken).ConfigureAwait(false);
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
            throw new InvalidOperationException($"Graph sender cannot send using '{target.DeliveryMethod}'.");
        }
        var accessToken = await ResolveAccessTokenAsync(target, cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Post, target.TargetUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return new TeamsDeliveryResult {
            DeliveryMethod = target.DeliveryMethod,
            TargetUri = target.TargetUri,
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

    private static async Task<string> ResolveAccessTokenAsync(TeamsMessageTarget target, CancellationToken cancellationToken) {
        if (!string.IsNullOrWhiteSpace(target.AccessToken)) {
            return target.AccessToken!;
        }

        if (target.AccessTokenProvider is not null) {
            var token = await target.AccessTokenProvider(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(token)) {
                return token;
            }
        }

        throw new InvalidOperationException("Graph targets require an access token.");
    }
}
