using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates one Teams list-card item.
/// </summary>
[Cmdlet(VerbsCommon.New, "CardListItem")]
[OutputType(typeof(TeamsListCardItem))]
public sealed class CmdletNewCardListItem : PSCmdlet {
    [Parameter(Mandatory = true)]
    public TeamsListCardItemKind Type { get; set; }

    [Parameter(Mandatory = false)]
    public string? Icon { get; set; }

    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? SubTitle { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("whois", "editOnline")]
    public string? TapAction { get; set; }

    [Parameter(Mandatory = false)]
    public TeamsCardButtonActionType? TapType { get; set; }

    [Parameter(Mandatory = false)]
    public string? TapValue { get; set; }

    protected override void ProcessRecord() {
        WriteObject(new TeamsListCardItem {
            Kind = Type,
            Icon = Icon,
            Title = Title,
            SubTitle = SubTitle,
            TapAction = TapAction,
            TapType = TapType,
            TapValue = TapValue
        });
    }
}
