using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsMessage")]
[OutputType(typeof(TeamsMessageRequest))]
public sealed class CmdletNewTeamsMessage : PSCmdlet {
    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false)]
    public string? Summary { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsAdaptiveCard? AdaptiveCard { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsMessageSection[] Sections { get; set; } = Array.Empty<TeamsMessageSection>();

    [Alias("Color")]
    [Parameter(Mandatory = false)]
    public string? ThemeColor { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter HideOriginalBody { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter UseConnectorCardFormat { get; set; }

    protected override void ProcessRecord() {
        var sections = Sections ?? Array.Empty<TeamsMessageSection>();
        var request = new TeamsMessageRequest {
            Title = Title,
            Text = Text,
            Summary = Summary,
            AdaptiveCard = AdaptiveCard,
            ThemeColor = ResolveThemeColor(),
            HideOriginalBody = HideOriginalBody.IsPresent,
            UseConnectorCardFormat = ShouldUseConnectorCardFormat(sections)
        };

        foreach (var section in sections) {
            if (section is not null) {
                request.Sections.Add(section);
            }
        }

        WriteObject(request);
    }

    private string? ResolveThemeColor() {
        if (string.IsNullOrWhiteSpace(ThemeColor)) {
            return null;
        }

        return TeamsColorUtility.NormalizeToHex(ThemeColor);
    }

    private bool ShouldUseConnectorCardFormat(TeamsMessageSection[] sections) {
        if (UseConnectorCardFormat.IsPresent) {
            return true;
        }

        return AdaptiveCard is null &&
               (sections.Length > 0 ||
                !string.IsNullOrWhiteSpace(ThemeColor) ||
                HideOriginalBody.IsPresent);
    }
}
