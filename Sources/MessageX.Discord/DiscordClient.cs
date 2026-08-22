using System.Net.Http;

namespace MessageX.Discord;

/// <summary>Routes typed Discord messages to incoming-webhook or bot REST senders.</summary>
public sealed class DiscordClient : IDisposable {
    private readonly IReadOnlyList<IDiscordMessageSender> _senders;
    private readonly IReadOnlyList<IDisposable> _ownedDisposables;

    /// <summary>Creates an incoming-webhook client with default transport behavior.</summary>
    public DiscordClient()
        : this(connection: null, options: null, useSharedTransport: true) {
    }

    /// <summary>Creates an incoming-webhook client with configured transport behavior.</summary>
    public DiscordClient(MessageHttpTransportOptions options)
        : this(connection: null, options, useSharedTransport: false) {
    }

    /// <summary>Creates a webhook and authenticated bot REST client.</summary>
    public DiscordClient(DiscordConnection connection, MessageHttpTransportOptions? options = null)
        : this(connection, options, useSharedTransport: options is null) {
    }

    /// <summary>Creates a client over caller-managed senders.</summary>
    public DiscordClient(IEnumerable<IDiscordMessageSender> senders) {
        _senders = senders?.ToArray() ?? throw new ArgumentNullException(nameof(senders));
        _ownedDisposables = Array.Empty<IDisposable>();
    }

    private DiscordClient(
        DiscordConnection? connection,
        MessageHttpTransportOptions? options,
        bool useSharedTransport) {
        var senders = new List<IDiscordMessageSender>();
        var disposables = new List<IDisposable>();
        var webhook = useSharedTransport
            ? new DiscordIncomingWebhookSender(DiscordHttpClientPool.Shared)
            : new DiscordIncomingWebhookSender(options ?? throw new ArgumentNullException(nameof(options)));
        senders.Add(webhook);
        if (!useSharedTransport) {
            disposables.Add(webhook);
        }
        if (connection is not null) {
            var bot = useSharedTransport
                ? new DiscordBotMessageSender(connection, DiscordHttpClientPool.Shared)
                : new DiscordBotMessageSender(connection, options!);
            senders.Add(bot);
            if (!useSharedTransport) {
                disposables.Add(bot);
            }
        }
        _senders = senders;
        _ownedDisposables = disposables;
    }

    /// <summary>Sends a message using the sender registered for the target transport.</summary>
    public Task<DiscordDeliveryResult> SendAsync(
        DiscordMessageRequest message,
        DiscordMessageTarget target,
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
                $"No Discord sender is configured for delivery method '{target.DeliveryMethod}'. " +
                "Authenticated bot targets require a DiscordConnection.");
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
