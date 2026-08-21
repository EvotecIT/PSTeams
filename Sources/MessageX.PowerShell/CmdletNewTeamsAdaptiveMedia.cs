using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveMedia")]
[OutputType(typeof(TeamsAdaptiveMedia))]
public sealed class CmdletNewTeamsAdaptiveMedia : PSCmdlet {
    [Parameter(Mandatory = true)]
    public TeamsAdaptiveMediaSource[] Sources { get; set; } = Array.Empty<TeamsAdaptiveMediaSource>();

    [Parameter(Mandatory = false)]
    public string? PosterUrl { get; set; }

    [Parameter(Mandatory = false)]
    public string? AlternateText { get; set; }

    [Parameter(Mandatory = false)]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

    [Parameter(Mandatory = false)]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    public string? Id { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Hidden { get; set; }

    protected override void ProcessRecord() {
        var media = new TeamsAdaptiveMedia {
            Poster = PosterUrl,
            AltText = AlternateText,
            Spacing = Spacing,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height,
            Id = Id
        };

        if (MyInvocation.BoundParameters.ContainsKey(nameof(Separator))) {
            media.Separator = Separator.IsPresent;
        }

        if (Hidden.IsPresent) {
            media.IsVisible = false;
        }

        foreach (var source in Sources ?? Array.Empty<TeamsAdaptiveMediaSource>()) {
            if (source is not null) {
                media.Sources.Add(source);
            }
        }

        WriteObject(media);
    }
}
