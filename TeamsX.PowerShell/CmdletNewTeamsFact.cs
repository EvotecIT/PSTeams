using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a connector-card fact item.
/// </summary>
/// <example>
/// <summary>Create a status fact for a Teams section</summary>
/// <code>New-TeamsFact -Name 'Status' -Value 'Failed'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "TeamsFact")]
[Alias("TeamsFact")]
[OutputType(typeof(TeamsMessageFact))]
public sealed class CmdletNewTeamsFact : PSCmdlet {
    /// <summary>Fact label displayed in the section.</summary>
    [Parameter(Mandatory = false, Position = 0)]
    public string? Name { get; set; }

    /// <summary>Fact value displayed beside the label.</summary>
    [Parameter(Mandatory = false, Position = 1)]
    public string? Value { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsMessageFact {
            Name = Name,
            Value = Value
        });
    }
}
