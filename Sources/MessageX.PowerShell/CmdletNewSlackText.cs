using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates typed Slack plain-text or mrkdwn content.</summary>
[Cmdlet(VerbsCommon.New, "SlackText", DefaultParameterSetName = "Markdown")]
[OutputType(typeof(SlackTextObject))]
public sealed class CmdletNewSlackText : PSCmdlet {
    /// <summary>Slack mrkdwn content.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Markdown")]
    public string Markdown { get; set; } = string.Empty;

    /// <summary>Slack plain-text content.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PlainText")]
    public string PlainText { get; set; } = string.Empty;

    /// <summary>Disables automatic link and mention conversion for mrkdwn.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "Markdown")]
    public SwitchParameter Verbatim { get; set; }

    /// <summary>Requests emoji conversion for plain text.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "PlainText")]
    public SwitchParameter Emoji { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(ParameterSetName == "PlainText"
            ? SlackTextObject.Plain(PlainText, Emoji.IsPresent ? true : null)
            : SlackTextObject.Markdown(Markdown, Verbatim.IsPresent ? true : null));
    }
}
