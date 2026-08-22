using System.Net.Http;

namespace MessageX.Slack;

/// <summary>Routes typed Slack messages to incoming-webhook or Web API senders.</summary>
public sealed class SlackClient : IDisposable {
    private readonly IReadOnlyList<ISlackMessageSender> _senders;
    private readonly IReadOnlyList<IDisposable> _ownedDisposables;

    /// <summary>Creates an incoming-webhook client with default transport behavior.</summary>
    public SlackClient()
        : this(connection: null, options: null, useSharedTransport: true) {
    }

    /// <summary>Creates an incoming-webhook client with configured transport behavior.</summary>
    public SlackClient(MessageHttpTransportOptions options)
        : this(connection: null, options, useSharedTransport: false) {
    }

    /// <summary>Creates a webhook and authenticated Web API client.</summary>
    public SlackClient(SlackConnection connection, MessageHttpTransportOptions? options = null)
        : this(connection, options, useSharedTransport: options is null) {
    }

    /// <summary>Creates a client over caller-managed senders.</summary>
    public SlackClient(IEnumerable<ISlackMessageSender> senders) {
        _senders = senders?.ToArray() ?? throw new ArgumentNullException(nameof(senders));
        _ownedDisposables = Array.Empty<IDisposable>();
    }

    private SlackClient(
        SlackConnection? connection,
        MessageHttpTransportOptions? options,
        bool useSharedTransport) {
        var senders = new List<ISlackMessageSender>();
        var disposables = new List<IDisposable>();

        var webhook = useSharedTransport
            ? new SlackIncomingWebhookSender(SlackHttpClientPool.Shared)
            : new SlackIncomingWebhookSender(options ?? throw new ArgumentNullException(nameof(options)));
        senders.Add(webhook);
        if (!useSharedTransport) {
            disposables.Add(webhook);
        }
        if (connection is not null) {
            var webApi = useSharedTransport
                ? new SlackWebApiMessageSender(connection, SlackHttpClientPool.Shared)
                : new SlackWebApiMessageSender(connection, options!);
            senders.Add(webApi);
            if (!useSharedTransport) {
                disposables.Add(webApi);
            }
        }

        _senders = senders;
        _ownedDisposables = disposables;
    }

    /// <summary>Sends a message using the sender registered for the target transport.</summary>
    public Task<SlackDeliveryResult> SendAsync(
        SlackMessageRequest message,
        SlackMessageTarget target,
        CancellationToken cancellationToken = default) {
        if (message is null) {
            throw new ArgumentNullException(nameof(message));
        }
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }

        var sender = _senders.FirstOrDefault(candidate => candidate.CanSend(target.DeliveryMethod));
        if (sender is null) {
            throw new InvalidOperationException(
                $"No Slack sender is configured for delivery method '{target.DeliveryMethod}'. " +
                "Authenticated Web API targets require a SlackConnection.");
        }
        return sender.SendAsync(message, target, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose() {
        foreach (var disposable in _ownedDisposables) {
            disposable.Dispose();
        }
    }
}
