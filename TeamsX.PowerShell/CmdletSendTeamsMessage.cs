using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommunications.Send, "TeamsMessage", SupportsShouldProcess = true)]
[OutputType(typeof(TeamsDeliveryResult))]
public sealed class CmdletSendTeamsMessage : PSCmdlet {
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
        if (!ShouldProcess(Target.TargetUri.ToString(), $"Send Teams message using {Target.DeliveryMethod}")) {
            return;
        }

        var client = new TeamsClient();
        var result = client.SendAsync(Message, Target).GetAwaiter().GetResult();

        if (PassThru) {
            WriteObject(result);
        }
    }
}
