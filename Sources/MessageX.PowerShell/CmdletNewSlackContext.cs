using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack Block Kit text context row.</summary>
/// <example>
/// <summary>Create a context row with deployment metadata</summary>
/// <code>New-SlackContext -Elements (New-SlackText -Markdown '*Environment:* Production') -BlockId 'deployment-context'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackContext")]
[OutputType(typeof(SlackContextBlock))]
public sealed class CmdletNewSlackContext : PSCmdlet {
    /// <summary>Plain-text or mrkdwn context elements.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateCount(1, 10)]
    public SlackTextObject[] Elements { get; set; } = Array.Empty<SlackTextObject>();

    /// <summary>Optional unique block identifier.</summary>
    [Parameter]
    public string? BlockId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var block = new SlackContextBlock { BlockId = BlockId };
        foreach (var element in Elements) {
            if (element is not null) {
                block.Elements.Add(element);
            }
        }
        WriteObject(block);
    }
}
