using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack modal plain-text input element.</summary>
/// <example>
/// <summary>Create a bounded multiline reason input</summary>
/// <code>New-SlackPlainTextInput -ActionId 'reason' -Multiline -MaximumLength 500 -Placeholder 'Explain the change'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackPlainTextInput")]
[OutputType(typeof(SlackPlainTextInputElement))]
public sealed class CmdletNewSlackPlainTextInput : PSCmdlet {
    /// <summary>Application-defined action identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string ActionId { get; set; } = string.Empty;

    /// <summary>Initial value.</summary>
    [Parameter]
    public string? InitialValue { get; set; }

    /// <summary>Allows multiple input lines.</summary>
    [Parameter]
    public SwitchParameter Multiline { get; set; }

    /// <summary>Minimum accepted length.</summary>
    [Parameter]
    [ValidateRange(0, 3000)]
    public int? MinimumLength { get; set; }

    /// <summary>Maximum accepted length.</summary>
    [Parameter]
    [ValidateRange(1, 3000)]
    public int? MaximumLength { get; set; }

    /// <summary>Optional placeholder.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new SlackPlainTextInputElement {
            ActionId = ActionId,
            InitialValue = InitialValue,
            Multiline = Multiline.IsPresent,
            MinimumLength = MinimumLength,
            MaximumLength = MaximumLength,
            Placeholder = Placeholder is null ? null : SlackTextObject.Plain(Placeholder)
        });
    }
}
