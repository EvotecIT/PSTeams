namespace MessageX.Discord;

/// <summary>Sends Discord messages through one supported transport.</summary>
public interface IDiscordMessageSender : IMessageSender<DiscordMessageRequest, DiscordMessageTarget, DiscordDeliveryResult> {
    /// <summary>Whether this sender supports the specified transport.</summary>
    bool CanSend(DiscordDeliveryMethod deliveryMethod);
}
