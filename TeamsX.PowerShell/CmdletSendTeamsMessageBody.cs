using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Sends a raw Teams message payload body to an incoming webhook.
/// </summary>
[Cmdlet(VerbsCommunications.Send, "TeamsMessageBody", SupportsShouldProcess = true)]
[Alias("TeamsMessageBody")]
[OutputType(typeof(string))]
public sealed class CmdletSendTeamsMessageBody : PSCmdlet {
    [Alias("TeamsID", "Url")]
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;

    [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true)]
    public string? Body { get; set; }

    [Alias("Suppress")]
    [Parameter(Mandatory = false)]
    public bool Supress { get; set; } = true;

    [Parameter(Mandatory = false)]
    public SwitchParameter Wrap { get; set; }

    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    protected override void ProcessRecord() {
        var jsonBody = Wrap.IsPresent
            ? WrapMessageBody(Body)
            : Body ?? string.Empty;

        WriteVerbose($"Send-TeamsMessageBody - Body {jsonBody}");

        if (!ShouldProcess(Uri.Host, "Send Teams message body using IncomingWebhook")) {
            if (!Supress) {
                WriteObject(jsonBody);
            }

            return;
        }

        using var clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(Proxy);
        var target = TeamsMessageTarget.ForIncomingWebhook(Uri);
        var result = clientLease.Client.SendJsonAsync(jsonBody, target).GetAwaiter().GetResult();

        WriteVerbose($"Send-TeamsMessageBody - Execute {result.ResponseBody}");
        TeamsPowerShellDeliverySupport.WriteDeliveryIssue(this, result, "Send-TeamsMessageBody");

        if (!Supress) {
            WriteObject(jsonBody);
        }
    }

    private static string WrapMessageBody(string? body) {
        var trimmedBody = string.IsNullOrWhiteSpace(body)
            ? "null"
            : body!.Trim();

        return $"{{\"type\":\"message\",\"attachments\":[{trimmedBody}]}}";
    }
}
