using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack Block Kit button.</summary>
/// <example>
/// <summary>Create an approval button</summary>
/// <code>New-SlackButton -Text 'Approve' -ActionId 'approve' -Value 'release-42' -Style Primary</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackButton")]
[OutputType(typeof(SlackButtonElement))]
public sealed class CmdletNewSlackButton : PSCmdlet {
    /// <summary>User-visible plain-text label.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Application-defined action identifier.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string ActionId { get; set; } = string.Empty;

    /// <summary>Optional application-defined interaction value.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>Optional external HTTPS URL.</summary>
    [Parameter]
    public Uri? Url { get; set; }

    /// <summary>Visual button style.</summary>
    [Parameter]
    public SlackButtonStyle Style { get; set; }

    /// <summary>Optional accessibility label.</summary>
    [Parameter]
    public string? AccessibilityLabel { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new SlackButtonElement {
            Text = SlackTextObject.Plain(Text),
            ActionId = ActionId,
            Value = Value,
            Url = Url,
            Style = Style,
            AccessibilityLabel = AccessibilityLabel
        });
    }
}
