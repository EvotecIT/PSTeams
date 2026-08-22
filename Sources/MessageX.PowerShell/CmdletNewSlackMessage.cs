using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a provider-native Slack message.</summary>
[Cmdlet(VerbsCommon.New, "SlackMessage")]
[OutputType(typeof(SlackMessageRequest))]
public sealed class CmdletNewSlackMessage : PSCmdlet {
    /// <summary>Top-level message text and accessibility fallback for blocks.</summary>
    [Parameter(Mandatory = false, Position = 0)]
    public string? Text { get; set; }

    /// <summary>Slack Block Kit blocks.</summary>
    [Parameter(Mandatory = false)]
    public SlackBlock[] Blocks { get; set; } = Array.Empty<SlackBlock>();

    /// <summary>Parent Slack message timestamp for a thread reply.</summary>
    [Parameter(Mandatory = false)]
    public string? ThreadTimestamp { get; set; }

    /// <summary>Broadcasts a thread reply to the conversation.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter ReplyBroadcast { get; set; }

    /// <summary>Controls link unfurling when explicitly supplied.</summary>
    [Parameter(Mandatory = false)]
    public bool? UnfurlLinks { get; set; }

    /// <summary>Controls media unfurling when explicitly supplied.</summary>
    [Parameter(Mandatory = false)]
    public bool? UnfurlMedia { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var message = new SlackMessageRequest {
            Text = Text,
            ThreadTimestamp = ThreadTimestamp,
            ReplyBroadcast = ReplyBroadcast.IsPresent,
            UnfurlLinks = UnfurlLinks,
            UnfurlMedia = UnfurlMedia
        };
        foreach (var block in Blocks ?? Array.Empty<SlackBlock>()) {
            if (block is not null) {
                message.Blocks.Add(block);
            }
        }
        WriteObject(message);
    }
}
