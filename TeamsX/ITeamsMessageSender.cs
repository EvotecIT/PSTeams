namespace TeamsX;

public interface ITeamsMessageSender {
    bool CanSend(TeamsDeliveryMethod deliveryMethod);

    Task<TeamsDeliveryResult> SendAsync(
        TeamsMessageRequest message,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default);
}
