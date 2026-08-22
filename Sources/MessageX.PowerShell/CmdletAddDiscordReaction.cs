using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Adds the authenticated Discord bot's reaction to a message.</summary>
/// <example>
/// <summary>Add an eyes reaction to a bot-owned message</summary>
/// <code>$connection = New-DiscordConnection -BotToken (Read-Host -AsSecureString); $target = New-DiscordChannelTarget -ChannelId '123456789012345678'; $message = New-DiscordMessage -Content 'Review ready'; $reference = (Send-DiscordMessage -Message $message -Target $target -Connection $connection -PassThru).Reference; Add-DiscordReaction -Reference $reference -Reaction '👀' -Connection $connection</code>
/// </example>
[Cmdlet(VerbsCommon.Add, "DiscordReaction", SupportsShouldProcess = true)]
[OutputType(typeof(DiscordDeliveryResult))]
public sealed class CmdletAddDiscordReaction : DiscordBotLifecycleCmdletBase {
    /// <summary>Durable Discord message reference.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public MessageReference Reference { get; set; } = null!;

    /// <summary>Unicode emoji or custom emoji coordinate in name:id form.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Reaction { get; set; } = string.Empty;

    /// <summary>Returns the typed operation result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        if (!ShouldProcess(Reference.ConversationId, $"Add Discord reaction {Reaction}")) {
            return;
        }
        var result = await LifecycleClient
            .AddReactionAsync(Reference, Reaction, CancelToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(DiscordPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Add-DiscordReaction"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
