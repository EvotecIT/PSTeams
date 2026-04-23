using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive line break backed by a newline text block.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveLineBreak")]
[OutputType(typeof(TeamsAdaptiveTextBlock))]
public sealed class CmdletNewAdaptiveLineBreak : PSCmdlet {
    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveTextBlock {
            Text = "\n"
        });
    }
}
