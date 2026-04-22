using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

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
        var request = new TeamsMessageRequest {
            Title = Title,
            Text = Text,
            Summary = Summary,
            AdaptiveCard = AdaptiveCard,
            ThemeColor = ResolveThemeColor(),
            HideOriginalBody = HideOriginalBody.IsPresent,
            UseConnectorCardFormat = UseConnectorCardFormat.IsPresent || (AdaptiveCard is null && Sections.Length > 0)
        };

        foreach (var section in Sections) {
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
}
