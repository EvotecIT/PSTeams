namespace MessageX.Teams;

public interface ITeamsMessageSender : IMessageSender<TeamsMessageRequest, TeamsMessageTarget, TeamsDeliveryResult> {
    bool CanSend(TeamsDeliveryMethod deliveryMethod);
}
