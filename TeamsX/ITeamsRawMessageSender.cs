namespace TeamsX;

public interface ITeamsRawMessageSender {
    Task<TeamsDeliveryResult> SendJsonAsync(
        string jsonBody,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default);
}
