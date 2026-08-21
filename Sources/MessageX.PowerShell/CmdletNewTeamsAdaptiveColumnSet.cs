using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsAdaptiveColumnSet")]
[OutputType(typeof(TeamsAdaptiveColumnSet))]
public sealed class CmdletNewTeamsAdaptiveColumnSet : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsAdaptiveColumn[] Columns { get; set; } = Array.Empty<TeamsAdaptiveColumn>();

    [Parameter(Mandatory = false)]
    [ValidateSet("Accent", "Default", "Emphasis", "Good", "Warning", "Attention")]
    public string? Style { get; set; }

    [Parameter(Mandatory = false)]
    public int MinimumHeight { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Bleed { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Left", "Center", "Right")]
    public string? HorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    protected override void ProcessRecord() {
        var columnSet = new TeamsAdaptiveColumnSet {
            Style = Style,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Bleed = Bleed.IsPresent ? true : null,
            Spacing = Spacing,
            Separator = Separator.IsPresent ? true : null,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height
        };

        foreach (var column in Columns) {
            if (column is not null) {
                columnSet.Columns.Add(column);
            }
        }

        WriteObject(columnSet);
    }
}
