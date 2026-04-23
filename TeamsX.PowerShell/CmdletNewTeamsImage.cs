using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a standard section image entry.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsImage")]
[Alias("TeamsImage")]
[OutputType(typeof(TeamsMessageImage))]
public sealed class CmdletNewTeamsImage : PSCmdlet {
    [Alias("Url", "Uri")]
    [Parameter(Mandatory = false, Position = 0)]
    public string? Link { get; set; }

    protected override void ProcessRecord() {
        if (string.IsNullOrWhiteSpace(Link)) {
            return;
        }

        WriteObject(new TeamsMessageImage {
            Image = Link,
            IsHeroImage = false
        });
    }
}
