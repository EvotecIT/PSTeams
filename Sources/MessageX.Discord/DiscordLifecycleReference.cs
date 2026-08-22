namespace MessageX.Discord;

internal static class DiscordLifecycleReference {
    public static Coordinates Validate(
        MessageReference reference,
        MessageCapabilities requiredCapability) {
        if (reference is null) {
            throw new ArgumentNullException(nameof(reference));
        }
        if (!string.Equals(reference.Provider, MessageProviders.Discord, StringComparison.Ordinal)) {
            throw new ArgumentException("A Discord message reference is required.", nameof(reference));
        }
        if ((reference.Capabilities & requiredCapability) != requiredCapability) {
            throw new InvalidOperationException(
                $"The Discord message reference does not support '{requiredCapability}'.");
        }

        var conversationId = DiscordSnowflake.Normalize(reference.ConversationId, nameof(reference));
        if (reference.ThreadId is not null &&
            !string.Equals(
                DiscordSnowflake.Normalize(reference.ThreadId, nameof(reference)),
                conversationId,
                StringComparison.Ordinal)) {
            throw new ArgumentException(
                "Discord thread references require matching conversation and thread coordinates.",
                nameof(reference));
        }

        return new Coordinates(
            conversationId,
            DiscordSnowflake.Normalize(reference.MessageId, nameof(reference)));
    }

    internal sealed class Coordinates {
        public Coordinates(string conversationId, string messageId) {
            ConversationId = conversationId;
            MessageId = messageId;
        }

        public string ConversationId { get; }

        public string MessageId { get; }
    }
}
