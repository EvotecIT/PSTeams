namespace MessageX.Slack;

/// <summary>Sends Slack messages through one supported transport.</summary>
public interface ISlackMessageSender : IMessageSender<SlackMessageRequest, SlackMessageTarget, SlackDeliveryResult> {
    /// <summary>Whether this sender supports the specified transport.</summary>
    bool CanSend(SlackDeliveryMethod deliveryMethod);
}
