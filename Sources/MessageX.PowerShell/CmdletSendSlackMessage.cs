using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Sends simple or typed messages through Slack incoming webhooks or the authenticated Web API.</summary>
[Cmdlet(VerbsCommunications.Send, "SlackMessage", SupportsShouldProcess = true, DefaultParameterSetName = "Typed")]
[OutputType(typeof(SlackDeliveryResult))]
public sealed class CmdletSendSlackMessage : SlackMessageCmdletBase {
    /// <summary>Typed Slack message.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Typed")]
    public SlackMessageRequest Message { get; set; } = null!;

    /// <summary>Typed Slack webhook or conversation target.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Typed")]
    public SlackMessageTarget Target { get; set; } = null!;

    /// <summary>Simple message text sent to an incoming webhook.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "WebhookText")]
    public string WebhookText { get; set; } = string.Empty;

    /// <summary>Secret incoming-webhook URI used by the simple webhook flow.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "WebhookText")]
    public Uri WebhookUri { get; set; } = null!;

    /// <summary>Simple message text sent through the Slack Web API.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ConversationText")]
    public string ConversationText { get; set; } = string.Empty;

    /// <summary>Slack channel, direct-message, multiparty-message, or user identifier.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ConversationText")]
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>Parent Slack timestamp when sending a simple conversation reply.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "ConversationText")]
    public string? ThreadTimestamp { get; set; }

    /// <summary>Broadcasts a simple conversation reply to the parent conversation.</summary>
    [Parameter(Mandatory = false, ParameterSetName = "ConversationText")]
    public SwitchParameter ReplyBroadcast { get; set; }

    /// <summary>Returns the typed delivery result.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        var (message, target) = ResolveRequest();
        EnsureConnectionAvailable(target);
        var safeTarget = ResolveSafeTarget(target);
        if (!ShouldProcess(safeTarget, $"Send Slack message using {target.DeliveryMethod}")) {
            return;
        }

        var result = await SendWithClientAsync(message, target).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-SlackMessage"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }

    private (SlackMessageRequest Message, SlackMessageTarget Target) ResolveRequest() {
        return ParameterSetName switch {
            "WebhookText" => (
                new SlackMessageRequest { Text = WebhookText },
                SlackMessageTarget.ForIncomingWebhook(WebhookUri)),
            "ConversationText" => (
                new SlackMessageRequest {
                    Text = ConversationText,
                    ThreadTimestamp = ThreadTimestamp,
                    ReplyBroadcast = ReplyBroadcast.IsPresent
                },
                SlackMessageTarget.ForConversation(ConversationId)),
            _ => (Message, Target)
        };
    }

    private void EnsureConnectionAvailable(SlackMessageTarget target) {
        if (target.DeliveryMethod == SlackDeliveryMethod.WebApi && Connection is null) {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidOperationException("A SlackConnection is required for authenticated conversation targets."),
                "SlackConnectionRequired",
                ErrorCategory.AuthenticationError,
                target.ConversationId));
        }
    }

    private static string ResolveSafeTarget(SlackMessageTarget target) {
        return target.ToString();
    }
}
