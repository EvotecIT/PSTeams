using System.Text.Json;

namespace MessageX.Discord;

/// <summary>Creates valid immediate Discord interaction acknowledgements.</summary>
public static class DiscordInteractionAcknowledgement {
    /// <summary>Creates the endpoint-validation PONG response.</summary>
    public static MessageAcknowledgement Pong() => Create(new Dictionary<string, object?> {
        ["type"] = 1
    });

    /// <summary>Creates a deferred channel-message response for asynchronous command or modal work.</summary>
    public static MessageAcknowledgement DeferredMessage(bool ephemeral = false) {
        var payload = new Dictionary<string, object?> {
            ["type"] = 5
        };
        if (ephemeral) {
            payload["data"] = new Dictionary<string, object?> {
                ["flags"] = 64
            };
        }
        return Create(payload);
    }

    /// <summary>Creates a deferred update response for an existing component message.</summary>
    public static MessageAcknowledgement DeferredUpdate() => Create(new Dictionary<string, object?> {
        ["type"] = 6
    });

    /// <summary>Creates an immediate empty autocomplete response.</summary>
    public static MessageAcknowledgement EmptyAutocomplete() => Create(new Dictionary<string, object?> {
        ["type"] = 8,
        ["data"] = new Dictionary<string, object?> {
            ["choices"] = Array.Empty<object>()
        }
    });

    private static MessageAcknowledgement Create(IReadOnlyDictionary<string, object?> payload) =>
        new(
            200,
            "application/json; charset=utf-8",
            JsonSerializer.SerializeToUtf8Bytes(payload));
}
