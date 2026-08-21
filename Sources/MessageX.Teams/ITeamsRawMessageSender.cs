namespace MessageX.Teams;

public interface ITeamsRawMessageSender : ITeamsMessageSender {
    Task<TeamsDeliveryResult> SendJsonAsync(
        string jsonBody,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default);
}
