using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Retrieves an application-owned Discord message through a bot or owning webhook.</summary>
[Cmdlet(VerbsCommon.Get, "DiscordMessage", DefaultParameterSetName = "Bot")]
[OutputType(typeof(DiscordRetrievedMessage))]
public sealed class CmdletGetDiscordMessage : DiscordMessageLifecycleCmdletBase {
    /// <summary>Durable Discord message reference.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Bot")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Webhook")]
    public MessageReference Reference { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        var message = UsesBot
            ? await BotClient.GetAsync(Reference, CancelToken).ConfigureAwait(false)
            : await WebhookClient.GetAsync(Reference, CancelToken).ConfigureAwait(false);
        WriteObject(message);
    }
}
