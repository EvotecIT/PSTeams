using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive fact backed by the TeamsX model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveFact")]
[OutputType(typeof(TeamsAdaptiveFact))]
public sealed class CmdletNewAdaptiveFact : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public string? Value { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveFact {
            Title = Title ?? string.Empty,
            Value = Value ?? string.Empty
        });
    }
}
