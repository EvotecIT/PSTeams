using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a connector-card fact item.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsFact")]
[Alias("TeamsFact")]
[OutputType(typeof(TeamsMessageFact))]
public sealed class CmdletNewTeamsFact : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Name { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public string? Value { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageFact {
            Name = Name,
            Value = Value
        });
    }
}
