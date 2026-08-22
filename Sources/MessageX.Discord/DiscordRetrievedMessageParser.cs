using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace MessageX.Discord;

internal static class DiscordRetrievedMessageParser {
    public static DiscordRetrievedMessage Parse(
        HttpResponseMessage response,
        string responseBody,
        MessageReference source) {
        if (!response.IsSuccessStatusCode) {
            throw new MessageDeliveryException(
                "Discord message retrieval failed.",
                DiscordHttpResponseSupport.Classify((int)response.StatusCode));
        }
        try {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var id = root.GetProperty("id").GetString();
            var channelId = root.GetProperty("channel_id").GetString();
            if (!DiscordSnowflake.TryNormalize(id, out var normalizedId) ||
                !DiscordSnowflake.TryNormalize(channelId, out var normalizedChannelId) ||
                !string.Equals(normalizedId, source.MessageId, StringComparison.Ordinal) ||
                !string.Equals(normalizedChannelId, source.ConversationId, StringComparison.Ordinal)) {
                throw new JsonException();
            }
            var timestamp = root.TryGetProperty("timestamp", out var timestampElement) &&
                timestampElement.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(
                    timestampElement.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var parsedTimestamp)
                ? parsedTimestamp
                : (DateTimeOffset?)null;
            return new DiscordRetrievedMessage {
                Content = root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String
                    ? content.GetString()
                    : null,
                Timestamp = timestamp,
                Reference = new MessageReference(MessageProviders.Discord, normalizedId) {
                    InstallationId = source.InstallationId,
                    ScopeId = source.ScopeId,
                    ConversationId = normalizedChannelId,
                    ThreadId = source.ThreadId,
                    Timestamp = timestamp,
                    CorrelationId = DiscordHttpResponseSupport.ReadHeader(response, "cf-ray"),
                    Capabilities = source.Capabilities
                }
            };
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException) {
            throw new MessageDeliveryException(
                "Discord returned an invalid message response.",
                MessageErrorKind.Transient);
        }
    }
}
