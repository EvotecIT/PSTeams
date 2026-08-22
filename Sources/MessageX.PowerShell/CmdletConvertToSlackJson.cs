using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Serializes a typed Slack message using the exact provider payload contract.</summary>
[Cmdlet(VerbsData.ConvertTo, "SlackJson")]
[OutputType(typeof(string))]
public sealed class CmdletConvertToSlackJson : PSCmdlet {
    /// <summary>Slack message to serialize.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public SlackMessageRequest Message { get; set; } = null!;

    /// <summary>Slack target whose transport determines the payload envelope.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public SlackMessageTarget Target { get; set; } = null!;

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(SlackJsonSerializer.Serialize(Message, Target));
    }
}
