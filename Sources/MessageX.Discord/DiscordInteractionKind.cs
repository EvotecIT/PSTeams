namespace MessageX.Discord;

/// <summary>Supported Discord HTTP interaction type.</summary>
public enum DiscordInteractionKind {
    /// <summary>Endpoint-validation ping.</summary>
    Ping = 1,
    /// <summary>Application command.</summary>
    ApplicationCommand = 2,
    /// <summary>Message component.</summary>
    MessageComponent = 3,
    /// <summary>Application-command autocomplete request.</summary>
    Autocomplete = 4,
    /// <summary>Modal submission.</summary>
    ModalSubmit = 5
}
