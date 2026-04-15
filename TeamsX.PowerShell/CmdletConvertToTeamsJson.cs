using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsData.ConvertTo, "TeamsJson")]
[OutputType(typeof(string))]
public sealed class CmdletConvertToTeamsJson : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public TeamsMessageRequest Message { get; set; } = null!;

    protected override void ProcessRecord() {
        WriteObject(WebhookMessageRenderer.Render(Message));
    }
}
