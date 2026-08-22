using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MessageX.Discord;

internal static class DiscordBotMessageOwnership {
    public static async Task VerifyAsync(
        HttpClient httpClient,
        DiscordConnection connection,
        DiscordLifecycleReference.Coordinates coordinates,
        CancellationToken cancellationToken) {
        using var identityRequest = CreateAuthorizedRequest(connection, HttpMethod.Get, "users/@me");
        var botId = await DiscordLifecycleHttp.ExecuteAsync(
            httpClient,
            identityRequest,
            "bot identity verification",
            (response, body) => ParseIdentity(response, body),
            cancellationToken).ConfigureAwait(false);

        using var messageRequest = CreateAuthorizedRequest(
            connection,
            HttpMethod.Get,
            $"channels/{coordinates.ConversationId}/messages/{coordinates.MessageId}");
        var authorId = await DiscordLifecycleHttp.ExecuteAsync(
            httpClient,
            messageRequest,
            "bot message ownership verification",
            (response, body) => ParseMessageAuthor(response, body, coordinates),
            cancellationToken).ConfigureAwait(false);

        if (!string.Equals(botId, authorId, StringComparison.Ordinal)) {
            throw new MessageDeliveryException(
                "Discord bot message deletion was refused because the message is not owned by the authenticated bot.",
                MessageErrorKind.Authorization);
        }
    }

    private static string ParseIdentity(HttpResponseMessage response, string body) {
        if (!response.IsSuccessStatusCode) {
            throw Failed("Discord bot identity verification failed.", response);
        }
        try {
            using var document = JsonDocument.Parse(body);
            if (TryReadSnowflake(document.RootElement, "id", out var id)) {
                return id;
            }
        }
        catch (JsonException) {
        }
        throw Invalid("Discord returned an invalid bot identity response.");
    }

    private static string ParseMessageAuthor(
        HttpResponseMessage response,
        string body,
        DiscordLifecycleReference.Coordinates coordinates) {
        if (!response.IsSuccessStatusCode) {
            throw Failed("Discord bot message ownership verification failed.", response);
        }
        try {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (!TryReadSnowflake(root, "id", out var messageId) ||
                !TryReadSnowflake(root, "channel_id", out var channelId) ||
                !string.Equals(messageId, coordinates.MessageId, StringComparison.Ordinal) ||
                !string.Equals(channelId, coordinates.ConversationId, StringComparison.Ordinal) ||
                !root.TryGetProperty("author", out var author) ||
                author.ValueKind != JsonValueKind.Object ||
                !TryReadSnowflake(author, "id", out var authorId)) {
                throw new JsonException();
            }
            return authorId;
        }
        catch (JsonException) {
            throw Invalid("Discord returned an invalid bot message ownership response.");
        }
    }

    private static bool TryReadSnowflake(JsonElement root, string name, out string id) {
        id = string.Empty;
        return root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.String &&
            DiscordSnowflake.TryNormalize(value.GetString(), out id);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        DiscordConnection connection,
        HttpMethod method,
        string relativeUri) {
        var request = new HttpRequestMessage(method, new Uri(DiscordConnection.DefaultApiBaseUri, relativeUri));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bot", connection.BotToken);
        return request;
    }

    private static MessageDeliveryException Failed(string message, HttpResponseMessage response) => new(
        message,
        DiscordHttpResponseSupport.Classify((int)response.StatusCode));

    private static MessageDeliveryException Invalid(string message) => new(
        message,
        MessageErrorKind.Transient);
}
