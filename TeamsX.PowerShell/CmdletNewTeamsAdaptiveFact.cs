using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveFact")]
[OutputType(typeof(TeamsAdaptiveFact))]
public sealed class CmdletNewTeamsAdaptiveFact : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public string Title { get; set; } = string.Empty;

    [Parameter(Mandatory = true, Position = 1)]
    public string Value { get; set; } = string.Empty;

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveFact {
            Title = Title,
            Value = Value
        });
    }
}
