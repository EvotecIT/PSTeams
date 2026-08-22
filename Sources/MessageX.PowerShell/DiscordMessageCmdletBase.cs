using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Owns one Discord client for the complete cmdlet pipeline lifecycle.</summary>
public abstract class DiscordMessageCmdletBase : MessageHttpCmdletBase {
    private DiscordClient? _client;

    /// <summary>Authenticated Discord bot connection used for channel, thread, and direct-message targets.</summary>
    [Parameter(Mandatory = false)]
    public DiscordConnection? Connection { get; set; }

    /// <summary>Creates one transport client after parameter binding is complete.</summary>
    protected override Task BeginProcessingAsync() {
        var options = CreateTransportOptions();
        var useSharedTransport = UsesDefaultTransport(options);
        _client = (Connection, useSharedTransport) switch {
            (null, true) => new DiscordClient(),
            (null, false) => new DiscordClient(options),
            (not null, true) => new DiscordClient(Connection),
            _ => new DiscordClient(Connection!, options)
        };
        return Task.CompletedTask;
    }

    /// <summary>Sends through the lifecycle-scoped Discord client.</summary>
    protected Task<DiscordDeliveryResult> SendWithClientAsync(
        DiscordMessageRequest message,
        DiscordMessageTarget target) {
        var client = _client ?? throw new InvalidOperationException(
            "The Discord client is not available outside the cmdlet processing lifecycle.");
        return client.SendAsync(message, target, CancelToken);
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        DisposeClient();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Dispose() {
        DisposeClient();
        base.Dispose();
    }

    private void DisposeClient() {
        var client = _client;
        _client = null;
        client?.Dispose();
    }
}
