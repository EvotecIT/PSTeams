using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Owns the selected Discord bot or webhook lifecycle client.</summary>
public abstract class DiscordMessageLifecycleCmdletBase : MessageHttpCmdletBase {
    private DiscordBotLifecycleClient? _botClient;
    private DiscordWebhookLifecycleClient? _webhookClient;

    /// <summary>Authenticated Discord bot connection.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "Bot")]
    public DiscordConnection? Connection { get; set; }

    /// <summary>Credential-bearing Discord webhook target kept only in memory.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "Webhook")]
    public DiscordMessageTarget? WebhookTarget { get; set; }

    /// <summary>Whether the bot parameter set is active.</summary>
    protected bool UsesBot => ParameterSetName == "Bot";

    /// <summary>Selected bot lifecycle client.</summary>
    protected DiscordBotLifecycleClient BotClient => _botClient ??
        throw new InvalidOperationException("The Discord bot lifecycle client is unavailable.");

    /// <summary>Selected webhook lifecycle client.</summary>
    protected DiscordWebhookLifecycleClient WebhookClient => _webhookClient ??
        throw new InvalidOperationException("The Discord webhook lifecycle client is unavailable.");

    /// <inheritdoc />
    protected override Task BeginProcessingAsync() {
        var options = CreateTransportOptions();
        if (UsesBot) {
            _botClient = new DiscordBotLifecycleClient(Connection!, options);
        } else {
            _webhookClient = new DiscordWebhookLifecycleClient(WebhookTarget!, options);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        DisposeClients();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Dispose() {
        DisposeClients();
        base.Dispose();
    }

    private void DisposeClients() {
        _botClient?.Dispose();
        _webhookClient?.Dispose();
        _botClient = null;
        _webhookClient = null;
    }
}
