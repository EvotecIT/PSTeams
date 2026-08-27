using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a typed Slack modal view.</summary>
/// <example>
/// <summary>Create an approval modal with one reason input</summary>
/// <code>$input = New-SlackPlainTextInput -ActionId 'reason' -Multiline; $block = New-SlackInput -Label 'Reason' -Element $input; New-SlackModal -CallbackId 'approval' -Title 'Approval' -Blocks $block -Submit 'Submit' -Close 'Cancel'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "SlackModal")]
[OutputType(typeof(SlackModalView))]
public sealed class CmdletNewSlackModal : PSCmdlet {
    /// <summary>Application-defined callback identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string CallbackId { get; set; } = string.Empty;

    /// <summary>Plain-text modal title.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Modal Block Kit blocks.</summary>
    [Parameter(Mandatory = true, Position = 2)]
    [ValidateCount(1, 100)]
    public SlackBlock[] Blocks { get; set; } = Array.Empty<SlackBlock>();

    /// <summary>Optional submit label.</summary>
    [Parameter]
    public string? Submit { get; set; }

    /// <summary>Optional close label.</summary>
    [Parameter]
    public string? Close { get; set; }

    /// <summary>Requests a view_closed interaction.</summary>
    [Parameter]
    public SwitchParameter NotifyOnClose { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var view = new SlackModalView {
            CallbackId = CallbackId,
            Title = SlackTextObject.Plain(Title),
            Submit = Submit is null ? null : SlackTextObject.Plain(Submit),
            Close = Close is null ? null : SlackTextObject.Plain(Close),
            NotifyOnClose = NotifyOnClose.IsPresent
        };
        foreach (var block in Blocks) {
            if (block is not null) {
                view.Blocks.Add(block);
            }
        }
        WriteObject(view);
    }
}
