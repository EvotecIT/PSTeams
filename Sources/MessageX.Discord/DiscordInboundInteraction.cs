using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Discord;

/// <summary>Verified Discord interaction with safe routing coordinates and transient provider data.</summary>
public sealed class DiscordInboundInteraction {
    /// <summary>Creates a safe persisted Discord interaction projection without transient provider capabilities.</summary>
    public DiscordInboundInteraction(
        DiscordInteractionKind kind,
        string name,
        string? installationOwnerId,
        string? locale,
        string? guildLocale,
        int? context,
        DiscordApplicationCommandType? commandType,
        string? targetId,
        string? applicationId = null) : this(
            kind,
            name,
            installationOwnerId,
            locale,
            guildLocale,
            context,
            commandType,
            targetId,
            applicationId,
            EmptyData,
            DiscordTransientInteractionContext.Unavailable) {
    }

    /// <summary>Creates a safe persisted Discord interaction projection with handler-useful provider data.</summary>
    [JsonConstructor]
    public DiscordInboundInteraction(
        DiscordInteractionKind kind,
        string name,
        string? installationOwnerId,
        string? locale,
        string? guildLocale,
        int? context,
        DiscordApplicationCommandType? commandType,
        string? targetId,
        JsonElement data,
        string? applicationId = null) : this(
            kind,
            name,
            installationOwnerId,
            locale,
            guildLocale,
            context,
            commandType,
            targetId,
            applicationId,
            data,
            DiscordTransientInteractionContext.Unavailable) {
    }

    internal DiscordInboundInteraction(
        DiscordInteractionKind kind,
        string name,
        string? installationOwnerId,
        string? locale,
        string? guildLocale,
        int? context,
        DiscordApplicationCommandType? commandType,
        string? targetId,
        string? applicationId,
        JsonElement data,
        DiscordTransientInteractionContext transientContext) {
        Kind = kind;
        Name = name;
        InstallationOwnerId = installationOwnerId;
        Locale = locale;
        GuildLocale = guildLocale;
        Context = context;
        CommandType = commandType;
        TargetId = targetId;
        ApplicationId = applicationId;
        Data = DiscordSafeInteractionData.Create(data);
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

    /// <summary>Target user or message identifier for a Discord context command.</summary>
    public string? TargetId { get; }

    /// <summary>Non-secret Discord application identifier required for durable interaction dispatch.</summary>
    public string? ApplicationId { get; }

    /// <summary>Safe provider-native interaction data, including bounded command options and modal inputs.</summary>
    public JsonElement Data { get; }

    /// <summary>Explicitly transient follow-up capability.</summary>
    [JsonIgnore]
    public DiscordTransientInteractionContext TransientContext { get; }

    private static readonly JsonElement EmptyData = CreateEmptyData();

    private static JsonElement CreateEmptyData() {
        using var document = JsonDocument.Parse("{}");
        return document.RootElement.Clone();
    }
}
