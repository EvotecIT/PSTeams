using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Slack Block Kit section.</summary>
[Cmdlet(VerbsCommon.New, "SlackSection", DefaultParameterSetName = "Markdown")]
[OutputType(typeof(SlackSectionBlock))]
public sealed class CmdletNewSlackSection : PSCmdlet {
    /// <summary>Markdown section text.</summary>
    [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Markdown")]
    public string Markdown { get; set; } = string.Empty;

    /// <summary>Plain section text.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PlainText")]
    public string PlainText { get; set; } = string.Empty;

    /// <summary>Typed Slack text object.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Typed")]
    public SlackTextObject TextObject { get; set; } = null!;

    /// <summary>Optional compact section fields.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "Markdown")]
    [Parameter(Mandatory = false, ParameterSetName = "PlainText")]
    [Parameter(Mandatory = false, ParameterSetName = "Typed")]
    public SlackTextObject[] Fields { get; set; } = Array.Empty<SlackTextObject>();

    /// <summary>Optional unique Slack block identifier.</summary>
    [Parameter(Mandatory = false)]
    public string? BlockId { get; set; }

    /// <summary>Requests that Slack initially expand long section text.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter Expand { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var text = ParameterSetName switch {
            "PlainText" => SlackTextObject.Plain(PlainText),
            "Typed" => TextObject,
            "Markdown" when !string.IsNullOrWhiteSpace(Markdown) => SlackTextObject.Markdown(Markdown),
            _ => null
        };
        var section = new SlackSectionBlock {
            Text = text,
            BlockId = BlockId,
            Expand = Expand.IsPresent ? true : null
        };
        foreach (var field in Fields ?? Array.Empty<SlackTextObject>()) {
            if (field is not null) {
                section.Fields.Add(field);
            }
        }
        if (section.Text is null && section.Fields.Count == 0) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentException("A Slack section requires text or at least one field.", nameof(Fields)),
                "SlackSectionContentRequired",
                ErrorCategory.InvalidArgument,
                Fields));
        }
        WriteObject(section);
    }
}
