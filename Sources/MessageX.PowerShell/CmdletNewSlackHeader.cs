using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack Block Kit header.</summary>
/// <example>
/// <summary>Create an incident header</summary>
/// <code>New-SlackHeader -Text 'Production incident' -BlockId 'incident-header'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackHeader")]
[OutputType(typeof(SlackHeaderBlock))]
public sealed class CmdletNewSlackHeader : PSCmdlet {
    /// <summary>Plain-text header.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional unique block identifier.</summary>
    [Parameter]
    public string? BlockId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new SlackHeaderBlock {
            Text = SlackTextObject.Plain(Text),
            BlockId = BlockId
        });
    }
}
