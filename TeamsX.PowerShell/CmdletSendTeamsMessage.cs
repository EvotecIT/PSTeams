using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommunications.Send, "TeamsMessage", SupportsShouldProcess = true)]
[OutputType(typeof(TeamsDeliveryResult))]
public sealed class CmdletSendTeamsMessage : PSCmdlet {
    private static readonly TeamsClient SharedClient = TeamsClient.Default;

    [Parameter(Mandatory = true, Position = 0)]
    public TeamsMessageRequest Message { get; set; } = null!;

    [Parameter(Mandatory = true, Position = 1)]
    public TeamsMessageTarget Target { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public SwitchParameter PassThru { get; set; }

    protected override void ProcessRecord() {
        ProcessTypedRecord();
    }

    private void ProcessTypedRecord() {
        if (!ShouldProcess(GetShouldProcessTarget(), $"Send Teams message using {Target.DeliveryMethod}")) {
            return;
        }

        var result = SharedClient.SendAsync(Message, Target).GetAwaiter().GetResult();

        if (!result.IsSuccessStatusCode) {
            WriteError(CreateDeliveryFailureError(result));
        }

        if (PassThru) {
            WriteObject(result);
        }
    }

    private string GetShouldProcessTarget() {
        if (!string.IsNullOrWhiteSpace(Target.DisplayName)) {
            return Target.DisplayName!;
        }

        return $"{Target.DeliveryMethod} target at {Target.TargetUri.Host}";
    }

    private ErrorRecord CreateDeliveryFailureError(TeamsDeliveryResult result) {
        var statusCode = result.StatusCode?.ToString() ?? "unknown";
        var message = $"Teams message delivery failed using {result.DeliveryMethod}. HTTP status: {statusCode}.";
        var error = new ErrorRecord(
            new InvalidOperationException(message),
            "TeamsMessageDeliveryFailed",
            ErrorCategory.ConnectionError,
            result.TargetUri) {
            ErrorDetails = new ErrorDetails(result.ResponseBody ?? message)
        };

        return error;
    }
}
