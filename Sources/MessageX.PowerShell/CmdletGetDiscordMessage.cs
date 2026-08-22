using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Retrieves an application-owned Discord message through a bot or owning webhook.</summary>
/// <example>
/// <summary>Retrieve a bot-owned Discord message</summary>
/// <code>$connection = New-DiscordConnection -BotToken (Read-Host -AsSecureString); $target = New-DiscordChannelTarget -ChannelId '123456789012345678'; $message = New-DiscordMessage -Content 'Current status'; $reference = (Send-DiscordMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Get-DiscordMessage -Reference $reference -Connection $connection</code>
/// </example>
/// <example>
/// <summary>Retrieve a message through its owning webhook</summary>
/// <code>$target = New-DiscordWebhookTarget -Uri $webhookUri; $message = New-DiscordMessage -Content 'Current status'; $reference = (Send-DiscordMessage -Message $message -Target $target -PassThru).Reference; Get-DiscordMessage -Reference $reference -WebhookTarget $target</code>
/// </example>
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
