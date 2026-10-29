namespace MessageX.Teams;

/// <summary>Sends Teams Workflow messages through a registered sender.</summary>
public sealed class TeamsClient {
    public static TeamsClient Default { get; } = new(new ITeamsMessageSender[] { WebhookTeamsMessageSender.Shared });

    private readonly IReadOnlyList<ITeamsMessageSender> _senders;

    public TeamsClient() : this(new ITeamsMessageSender[] { WebhookTeamsMessageSender.Shared }) { }

    public TeamsClient(IEnumerable<ITeamsMessageSender> senders) {
        _senders = senders?.ToArray() ?? throw new ArgumentNullException(nameof(senders));
    }

    public Task<TeamsDeliveryResult> SendAsync(
        TeamsMessageRequest message,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (target is null) throw new ArgumentNullException(nameof(target));
        var sender = _senders.FirstOrDefault(s => s.CanSend(target.DeliveryMethod));
        if (sender is null) {
            throw new InvalidOperationException($"No sender registered for delivery method '{target.DeliveryMethod}'.");
        }
        return sender.SendAsync(message, target, cancellationToken);
    }
}
