using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack Block Kit divider.</summary>
[Cmdlet(VerbsCommon.New, "SlackDivider")]
[OutputType(typeof(SlackDividerBlock))]
public sealed class CmdletNewSlackDivider : PSCmdlet {
    /// <summary>Optional unique Slack block identifier.</summary>
    [Parameter(Mandatory = false)]
    public string? BlockId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new SlackDividerBlock { BlockId = BlockId });
    }
}
