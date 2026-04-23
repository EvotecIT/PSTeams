namespace TeamsX;

public sealed class TeamsClient {
    public static TeamsClient Default { get; } = new(new ITeamsMessageSender[] { WebhookTeamsMessageSender.Shared, GraphTeamsMessageSender.Shared });

    private readonly IReadOnlyList<ITeamsMessageSender> _senders;

    public TeamsClient()
        : this(new ITeamsMessageSender[] { WebhookTeamsMessageSender.Shared, GraphTeamsMessageSender.Shared }) {
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

    public Task<TeamsDeliveryResult> SendAsync(
        TeamsHeroCard card,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        return SendWrapperCardAsync(TeamsWrapperCardRenderer.Render(card), target, cancellationToken);
    }

    public Task<TeamsDeliveryResult> SendAsync(
        TeamsThumbnailCard card,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        return SendWrapperCardAsync(TeamsWrapperCardRenderer.Render(card), target, cancellationToken);
    }

    public Task<TeamsDeliveryResult> SendAsync(
        TeamsListCard card,
        TeamsMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        return SendWrapperCardAsync(TeamsWrapperCardRenderer.Render(card), target, cancellationToken);
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

        var sender = _senders
            .OfType<ITeamsRawMessageSender>()
            .FirstOrDefault(s => s.CanSend(target.DeliveryMethod));
        if (sender is null) {
            throw new InvalidOperationException($"No raw sender registered for delivery method '{target.DeliveryMethod}'.");
        }

        return sender.SendJsonAsync(jsonBody, target, cancellationToken);
    }

    private Task<TeamsDeliveryResult> SendWrapperCardAsync(
        string attachmentBodyJson,
        TeamsMessageTarget target,
        CancellationToken cancellationToken) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }

        if (target.DeliveryMethod is not TeamsDeliveryMethod.IncomingWebhook and not TeamsDeliveryMethod.WorkflowWebhook) {
            throw new InvalidOperationException(
                $"Typed wrapper cards are currently supported only for incoming and workflow webhooks. Delivery method '{target.DeliveryMethod}' is not supported.");
        }

        var wrappedBody = TeamsWrapperCardRenderer.WrapAsMessage(attachmentBodyJson);
        return SendJsonAsync(wrappedBody, target, cancellationToken);
    }
}
