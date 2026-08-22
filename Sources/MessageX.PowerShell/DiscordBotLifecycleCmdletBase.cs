using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Owns one authenticated Discord bot lifecycle client.</summary>
public abstract class DiscordBotLifecycleCmdletBase : MessageHttpCmdletBase {
    private DiscordBotLifecycleClient? _client;

    /// <summary>Authenticated Discord bot connection.</summary>
    [Parameter(Mandatory = true)]
    public DiscordConnection Connection { get; set; } = null!;

    /// <summary>Discord bot lifecycle client available during cmdlet processing.</summary>
    protected DiscordBotLifecycleClient LifecycleClient => _client ??
        throw new InvalidOperationException("The Discord bot lifecycle client is unavailable.");

    /// <inheritdoc />
    protected override Task BeginProcessingAsync() {
        var options = CreateTransportOptions();
        _client = UsesDefaultTransport(options)
            ? new DiscordBotLifecycleClient(Connection)
            : new DiscordBotLifecycleClient(Connection, options);
        return Task.CompletedTask;
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
        _client?.Dispose();
        _client = null;
    }
}
