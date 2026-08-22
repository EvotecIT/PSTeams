using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Owns one Slack client for the complete cmdlet pipeline lifecycle.</summary>
public abstract class SlackMessageCmdletBase : MessageHttpCmdletBase {
    private SlackClient? _client;

    /// <summary>Authenticated Slack bot connection used for Web API targets.</summary>
    [Parameter(Mandatory = false)]
    public SlackConnection? Connection { get; set; }

    /// <summary>Creates one transport client after parameter binding is complete.</summary>
    protected override Task BeginProcessingAsync() {
        var options = CreateTransportOptions();
        var useSharedTransport = UsesDefaultTransport(options);
        _client = (Connection, useSharedTransport) switch {
            (null, true) => new SlackClient(),
            (null, false) => new SlackClient(options),
            (not null, true) => new SlackClient(Connection),
            _ => new SlackClient(Connection!, options)
        };
        return Task.CompletedTask;
    }

    /// <summary>Sends through the lifecycle-scoped Slack client.</summary>
    protected Task<SlackDeliveryResult> SendWithClientAsync(
        SlackMessageRequest message,
        SlackMessageTarget target) {
        var client = _client ?? throw new InvalidOperationException(
            "The Slack client is not available outside the cmdlet processing lifecycle.");
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
