using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive column set backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveColumnSet")]
[OutputType(typeof(TeamsAdaptiveColumnSet))]
public sealed class CmdletNewAdaptiveColumnSet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Columns { get; set; }

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
        if (Columns is null) {
            return;
        }

        var columnSet = new TeamsAdaptiveColumnSet {
            Style = Style,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Bleed = Bleed.IsPresent ? true : null,
            Spacing = Spacing,
            Separator = Separator.IsPresent ? true : null,
            HorizontalAlignment = HorizontalAlignment,
            Height = Height
        };

        foreach (var item in Columns.Invoke()) {
            var value = item is PSObject psObject ? psObject.BaseObject : item;
            if (value is TeamsAdaptiveColumn column) {
                columnSet.Columns.Add(column);
            }
        }

        if (columnSet.Columns.Count > 0) {
            WriteObject(columnSet);
        }
    }
}
