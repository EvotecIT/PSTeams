using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a hero-style markdown image entry for section text.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsBigImage")]
[Alias("TeamsBigImage")]
[OutputType(typeof(TeamsMessageImage))]
public sealed class CmdletNewTeamsBigImage : PSCmdlet {
    [Alias("Url", "Uri")]
    [Parameter(Mandatory = false, Position = 0)]
    public string? Link { get; set; }

    [Parameter(Mandatory = false)]
    public string AlternativeText { get; set; } = "Alternative Text";

    protected override void ProcessRecord() {
        if (string.IsNullOrWhiteSpace(Link)) {
            return;
        }

        WriteObject(new TeamsMessageImage {
            Image = $"![{AlternativeText}]({Link})",
            IsHeroImage = true
        });
    }
}
