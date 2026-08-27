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

    /// <summary>Creates an immediate autocomplete response from handler-produced typed choices.</summary>
    public static MessageAcknowledgement Autocomplete(IEnumerable<DiscordAutocompleteChoice> choices) {
        if (choices is null) {
            throw new ArgumentNullException(nameof(choices));
        }
        var values = choices.ToArray();
        if (values.Length > 25 || values.Any(value => value is null)) {
            throw new ArgumentException("Discord autocomplete responses support at most 25 choices.", nameof(choices));
        }
        return Create(new Dictionary<string, object?> {
            ["type"] = 8,
            ["data"] = new Dictionary<string, object?> {
                ["choices"] = values.Select(value => new Dictionary<string, object?> {
                    ["name"] = value.Name,
                    ["value"] = value.Value
                }).ToArray()
            }
        });
    }

    /// <summary>Opens a typed modal as the immediate response to a command or component interaction.</summary>
    public static MessageAcknowledgement Modal(DiscordModalRequest modal) {
        DiscordMessageValidator.ValidateModal(modal);
        return Create(new Dictionary<string, object?> {
            ["type"] = 9,
            ["data"] = new Dictionary<string, object?> {
                ["custom_id"] = modal.CustomId,
                ["title"] = modal.Title,
                ["components"] = DiscordMessageRenderer.RenderComponents(modal.Components)
            }
        });
    }

    private static MessageAcknowledgement Create(IReadOnlyDictionary<string, object?> payload) =>
        new(
            200,
            "application/json; charset=utf-8",
            JsonSerializer.SerializeToUtf8Bytes(payload));
}
