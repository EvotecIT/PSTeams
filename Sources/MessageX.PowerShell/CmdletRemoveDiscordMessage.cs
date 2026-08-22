using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Deletes an application-owned Discord message through a bot or owning webhook.</summary>
/// <example>
/// <summary>Delete a bot-owned Discord message</summary>
/// <code>$connection = New-DiscordConnection -BotToken (Read-Host -AsSecureString); $target = New-DiscordChannelTarget -ChannelId '123456789012345678'; $message = New-DiscordMessage -Content 'Temporary notice'; $reference = (Send-DiscordMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Remove-DiscordMessage -Reference $reference -Connection $connection</code>
/// </example>
/// <example>
/// <summary>Delete a message through its owning webhook</summary>
/// <code>$target = New-DiscordWebhookTarget -Uri $webhookUri; $message = New-DiscordMessage -Content 'Temporary notice'; $reference = (Send-DiscordMessage -Message $message -Target $target -PassThru).Reference; Remove-DiscordMessage -Reference $reference -WebhookTarget $target</code>
/// </example>
[Cmdlet(VerbsCommon.Remove, "DiscordMessage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "Bot")]
[OutputType(typeof(DiscordDeliveryResult))]
public sealed class CmdletRemoveDiscordMessage : DiscordMessageLifecycleCmdletBase {
    /// <summary>Durable Discord message reference.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Bot")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Webhook")]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, "Delete Discord message")) {
            return;
        }
        var result = UsesBot
            ? await BotClient.DeleteAsync(Reference, CancelToken).ConfigureAwait(false)
            : await WebhookClient.DeleteAsync(Reference, CancelToken).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(DiscordPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Remove-DiscordMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
