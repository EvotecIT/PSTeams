namespace TeamsX;

public sealed class TeamsClient {
    private readonly IReadOnlyList<ITeamsMessageSender> _senders;

    public TeamsClient()
        : this(new ITeamsMessageSender[] { new WebhookTeamsMessageSender() }) {
    }

    public TeamsClient(IEnumerable<ITeamsMessageSender> senders) {
        if (senders is null) {
            throw new ArgumentNullException(nameof(senders));
        }

        _senders = senders.ToArray();
    }

    public Task<TeamsDeliveryResult> SendAsync(
        TeamsMessageRequest message,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (message is null) {
            throw new ArgumentNullException(nameof(message));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }

        var sender = _senders.FirstOrDefault(s => s.CanSend(target.DeliveryMethod));
        if (sender is null) {
            throw new InvalidOperationException($"No sender registered for delivery method '{target.DeliveryMethod}'.");
        }

        return sender.SendAsync(message, target, cancellationToken);
    }

    public Task<TeamsDeliveryResult> SendJsonAsync(
        string jsonBody,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (jsonBody is null) {
            throw new ArgumentNullException(nameof(jsonBody));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }

        var sender = _senders.FirstOrDefault(s => s.CanSend(target.DeliveryMethod)) as ITeamsRawMessageSender;
        if (sender is null) {
            throw new InvalidOperationException($"No raw sender registered for delivery method '{target.DeliveryMethod}'.");
        }

        return sender.SendJsonAsync(jsonBody, target, cancellationToken);
    }
}
