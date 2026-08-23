using System.Text.Json.Serialization;

namespace MessageX.Discord;

/// <summary>Verified Discord interaction with safe routing coordinates and transient provider data.</summary>
public sealed class DiscordInboundInteraction {
    /// <summary>
    /// Creates a safe volatile Discord interaction projection without provider data or transient capabilities.
    /// The durable codec rejects this reduced projection because it cannot reconstruct dispatchable interaction data.
    /// </summary>
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
            null,
            null,
            null,
            null,
            null,
            null,
            EmptyData,
            DiscordTransientInteractionContext.Unavailable) {
    }

    /// <summary>Creates a safe persisted Discord interaction projection with handler-useful provider data.</summary>
    public DiscordInboundInteraction(
        DiscordInteractionKind kind,
        string name,
        string? installationOwnerId,
        string? locale,
        string? guildLocale,
        int? context,
        DiscordApplicationCommandType? commandType,
        string? targetId,
        MessageDataValue data,
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
            null,
            null,
            null,
            null,
            null,
            null,
            data,
            DiscordTransientInteractionContext.Unavailable) {
    }

    /// <summary>Creates a safe persisted Discord interaction with its independently verified sender identity.</summary>
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
        MessageDataValue data,
        string? applicationId,
        string? userId,
        string? interactionId = null,
        string? guildId = null,
        string? channelId = null,
        int? channelType = null,
        string? messageId = null) : this(
            kind,
            name,
            installationOwnerId,
            locale,
            guildLocale,
            context,
            commandType,
            targetId,
            applicationId,
            userId,
            interactionId,
            guildId,
            channelId,
            channelType,
            messageId,
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
        string? userId,
        string? interactionId,
        string? guildId,
        string? channelId,
        int? channelType,
        string? messageId,
        MessageDataValue data,
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
        UserId = userId;
        InteractionId = interactionId;
        GuildId = guildId;
        ChannelId = channelId;
        ChannelType = channelType;
        MessageId = messageId;
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

    /// <summary>Invoking Discord user identity retained from the verified request.</summary>
    public string? UserId { get; }

    /// <summary>Verified Discord interaction identity.</summary>
    public string? InteractionId { get; }

    /// <summary>Verified Discord guild identity, when supplied.</summary>
    public string? GuildId { get; }

    /// <summary>Verified Discord channel identity, when supplied.</summary>
    public string? ChannelId { get; }

    /// <summary>Verified Discord channel type, when supplied.</summary>
    public int? ChannelType { get; }

    /// <summary>Verified provider message identity, when supplied.</summary>
    public string? MessageId { get; }

    /// <summary>Safe provider-native interaction data, including bounded command options and modal inputs.</summary>
    public MessageDataValue Data { get; }

    /// <summary>Explicitly transient follow-up capability.</summary>
    [JsonIgnore]
    public DiscordTransientInteractionContext TransientContext { get; }

    private static readonly MessageDataValue EmptyData =
        MessageDataValue.FromObject(Array.Empty<KeyValuePair<string, MessageDataValue>>());
}
