using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord incoming-webhook target, optionally within an existing thread.</summary>
[Cmdlet(VerbsCommon.New, "DiscordWebhookTarget")]
[OutputType(typeof(DiscordMessageTarget))]
public sealed class CmdletNewDiscordWebhookTarget : PSCmdlet {
    /// <summary>Secret Discord incoming-webhook URI.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;

    /// <summary>Optional existing thread identifier.</summary>
    [Parameter(Mandatory = false)]
    public string? ThreadId { get; set; }

    /// <summary>Optional safe display label.</summary>
    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(DiscordMessageTarget.ForIncomingWebhook(Uri, ThreadId, DisplayName));
    }
}
