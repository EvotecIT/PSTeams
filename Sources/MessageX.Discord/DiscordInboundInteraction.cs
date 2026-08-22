using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Discord;

/// <summary>Verified Discord interaction with safe routing coordinates and transient provider data.</summary>
public sealed class DiscordInboundInteraction {
    internal DiscordInboundInteraction(
        DiscordInteractionKind kind,
        string name,
        string? installationOwnerId,
        string? locale,
        string? guildLocale,
        int? context,
        DiscordApplicationCommandType? commandType,
        JsonElement data,
        DiscordTransientInteractionContext transientContext) {
        Kind = kind;
        Name = name;
        InstallationOwnerId = installationOwnerId;
        Locale = locale;
        GuildLocale = guildLocale;
        Context = context;
        CommandType = commandType;
        Data = data;
        TransientContext = transientContext;
    }

    /// <summary>Discord interaction type.</summary>
    public DiscordInteractionKind Kind { get; }

    /// <summary>Application command name or component/modal custom identifier.</summary>
    public string Name { get; }

    /// <summary>Guild or user installation owner identifier, when supplied.</summary>
    public string? InstallationOwnerId { get; }

    /// <summary>Invoking user locale.</summary>
    public string? Locale { get; }

    /// <summary>Guild locale, when supplied.</summary>
    public string? GuildLocale { get; }

    /// <summary>Discord interaction context value.</summary>
    public int? Context { get; }

    /// <summary>Application-command type when the interaction is a command or autocomplete request.</summary>
    public DiscordApplicationCommandType? CommandType { get; }

    /// <summary>Provider-native interaction data. May contain user input and is intentionally transient.</summary>
    [JsonIgnore]
    public JsonElement Data { get; }

    /// <summary>Explicitly transient follow-up capability.</summary>
    [JsonIgnore]
    public DiscordTransientInteractionContext TransientContext { get; }
}
