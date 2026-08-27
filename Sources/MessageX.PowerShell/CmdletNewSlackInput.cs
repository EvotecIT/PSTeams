using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack modal input block.</summary>
/// <example>
/// <summary>Create an input block for an approval reason</summary>
/// <code>$input = New-SlackPlainTextInput -ActionId 'reason' -Multiline -MaximumLength 500; New-SlackInput -Label 'Reason' -Element $input -BlockId 'reason-block'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackInput")]
[OutputType(typeof(SlackInputBlock))]
public sealed class CmdletNewSlackInput : PSCmdlet {
    /// <summary>Plain-text field label.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Input element.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public SlackBlockElement Element { get; set; } = null!;

    /// <summary>Allows the user to omit this input.</summary>
    [Parameter]
    public SwitchParameter Optional { get; set; }

    /// <summary>Optional plain-text hint.</summary>
    [Parameter]
    public string? Hint { get; set; }

    /// <summary>Optional unique block identifier.</summary>
    [Parameter]
    public string? BlockId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new SlackInputBlock {
            Label = SlackTextObject.Plain(Label),
            Element = Element,
            Optional = Optional.IsPresent,
            Hint = Hint is null ? null : SlackTextObject.Plain(Hint),
            BlockId = BlockId
        });
    }
}
