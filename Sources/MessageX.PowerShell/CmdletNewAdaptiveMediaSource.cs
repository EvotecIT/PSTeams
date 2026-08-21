using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive media source backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveMediaSource")]
[OutputType(typeof(TeamsAdaptiveMediaSource))]
public sealed class CmdletNewAdaptiveMediaSource : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public string? Type { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public string? Url { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsAdaptiveMediaSource {
            MimeType = Type ?? string.Empty,
            Url = Url ?? string.Empty
        });
    }
}
