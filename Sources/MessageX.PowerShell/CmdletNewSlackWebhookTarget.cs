using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a fixed-destination Slack incoming-webhook target.</summary>
[Cmdlet(VerbsCommon.New, "SlackWebhookTarget")]
[OutputType(typeof(SlackMessageTarget))]
public sealed class CmdletNewSlackWebhookTarget : PSCmdlet {
    /// <summary>Secret Slack incoming-webhook URI.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;

    /// <summary>Optional safe display label.</summary>
    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(SlackMessageTarget.ForIncomingWebhook(Uri, DisplayName));
    }
}
