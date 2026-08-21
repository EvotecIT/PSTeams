using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Sends a raw Teams message payload body to an incoming webhook.
/// </summary>
/// <remarks>Use this command for pre-rendered payloads. Prefer Send-TeamsMessage for typed message models.</remarks>
/// <example>
/// <summary>Send a pre-rendered Teams payload through a webhook</summary>
/// <code>$json | Send-TeamsMessageBody -Uri $workflowUrl</code>
/// </example>
[Cmdlet(VerbsCommunications.Send, "TeamsMessageBody", SupportsShouldProcess = true)]
[Alias("TeamsMessageBody")]
[OutputType(typeof(string))]
public sealed class CmdletSendTeamsMessageBody : AsyncPSCmdlet {
    /// <summary>HTTPS Teams incoming webhook or Workflows URL.</summary>
    [Alias("TeamsID", "Url")]
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Uri { get; set; } = null!;

    /// <summary>Pre-rendered JSON body to send.</summary>
    [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true)]
    public string? Body { get; set; }

    /// <summary>Suppresses the rendered JSON output after processing. The default is true.</summary>
    [Alias("Suppress")]
    [Parameter(Mandatory = false)]
    public bool Supress { get; set; } = true;

    /// <summary>Wraps an Adaptive Card attachment body in the Teams message envelope.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter Wrap { get; set; }

    /// <summary>HTTP proxy used for the request.</summary>
    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    protected override async Task ProcessRecordAsync() {
        var jsonBody = Wrap.IsPresent
            ? WrapMessageBody(Body)
            : Body ?? string.Empty;

        WriteVerbose($"Send-TeamsMessageBody - Prepared {jsonBody.Length} characters for {Uri.Host}.");

        if (!ShouldProcess(Uri.Host, "Send Teams message body using IncomingWebhook")) {
            if (!Supress) {
                WriteObject(jsonBody);
            }

            return;
        }

        using var clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(Proxy);
        var target = TeamsMessageTarget.ForIncomingWebhook(Uri);
        var result = await clientLease.Client.SendJsonAsync(jsonBody, target, CancelToken);

        WriteVerbose($"Send-TeamsMessageBody - Completed with HTTP status {result.StatusCode?.ToString() ?? "unknown"}.");
        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "Send-TeamsMessageBody"));
        }

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
