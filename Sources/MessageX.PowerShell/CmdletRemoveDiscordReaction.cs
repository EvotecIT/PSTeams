using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Removes the authenticated Discord bot's reaction from a message.</summary>
[Cmdlet(VerbsCommon.Remove, "DiscordReaction", SupportsShouldProcess = true)]
[OutputType(typeof(DiscordDeliveryResult))]
public sealed class CmdletRemoveDiscordReaction : DiscordBotLifecycleCmdletBase {
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
        if (!ShouldProcess(Reference.ConversationId, $"Remove Discord reaction {Reaction}")) {
            return;
        }
        var result = await LifecycleClient
            .RemoveReactionAsync(Reference, Reaction, CancelToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(DiscordPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Remove-DiscordReaction"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }
}
