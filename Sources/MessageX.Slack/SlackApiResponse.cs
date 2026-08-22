using System.Text.Json;

namespace MessageX.Slack;

internal sealed class SlackApiResponse {
    public static SlackApiResponse Invalid { get; } = new();

    public bool IsValid { get; private set; }

    public bool Ok { get; private set; }

    public string? Error { get; private set; }

    public string? Channel { get; private set; }

    public string? Timestamp { get; private set; }

    public bool? NoOp { get; private set; }

    public bool? AlreadyOpen { get; private set; }

    public bool IsConversationLookupMiss =>
        Ok && NoOp is true && AlreadyOpen is false && Channel is null;

    public static SlackApiResponse Parse(string responseBody) {
        try {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("ok", out var okElement) ||
                okElement.ValueKind is not JsonValueKind.True and not JsonValueKind.False) {
                return Invalid;
            }

            var ok = okElement.GetBoolean();
            var error = ReadString(root, "error");
            return new SlackApiResponse {
                IsValid = ok || !string.IsNullOrWhiteSpace(error),
                Ok = ok,
                Error = error,
                Channel = ReadChannel(root),
                Timestamp = ReadString(root, "ts"),
                NoOp = ReadBoolean(root, "no_op"),
                AlreadyOpen = ReadBoolean(root, "already_open")
            };
        }
        catch (JsonException) {
            return Invalid;
        }
    }

    private static string? ReadChannel(JsonElement root) {
        if (!root.TryGetProperty("channel", out var channel)) {
            return null;
        }
        if (channel.ValueKind == JsonValueKind.String) {
            return channel.GetString();
        }
        return channel.ValueKind == JsonValueKind.Object
            ? ReadString(channel, "id")
            : null;
    }

    private static string? ReadString(JsonElement root, string propertyName) {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static bool? ReadBoolean(JsonElement root, string propertyName) {
        if (!root.TryGetProperty(propertyName, out var value)) {
            return null;
        }
        return value.ValueKind switch {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }
}
