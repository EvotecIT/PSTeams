using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack Block Kit actions block.</summary>
/// <example>
/// <summary>Create an actions block containing an approval button</summary>
/// <code>$button = New-SlackButton -Text 'Approve' -ActionId 'approve' -Style Primary; New-SlackActions -Elements $button -BlockId 'approval-actions'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackActions")]
[OutputType(typeof(SlackActionsBlock))]
public sealed class CmdletNewSlackActions : PSCmdlet {
    /// <summary>Interactive Block Kit elements.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateCount(1, 25)]
    public SlackBlockElement[] Elements { get; set; } = Array.Empty<SlackBlockElement>();

    /// <summary>Optional unique block identifier.</summary>
    [Parameter]
    public string? BlockId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var block = new SlackActionsBlock { BlockId = BlockId };
        foreach (var element in Elements) {
            if (element is not null) {
                block.Elements.Add(element);
            }
        }
        WriteObject(block);
    }
}
