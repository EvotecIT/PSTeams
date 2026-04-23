using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a button for ListCard, HeroCard, and ThumbnailCard payloads.
/// </summary>
[Cmdlet(VerbsCommon.New, "CardListButton")]
[OutputType(typeof(TeamsCardButton))]
public sealed class CmdletNewCardListButton : PSCmdlet {
    [Parameter(Mandatory = false)]
    public TeamsCardButtonActionType Type { get; set; }

    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? Value { get; set; }

    [Parameter(Mandatory = false)]
    public string? Image { get; set; }

    protected override void ProcessRecord() {
        if (!string.IsNullOrWhiteSpace(Image)) {
            WriteWarning("Using Image for Buttons while technically supported by Teams, it's not supported by Teams Connectors. Leaving this in place just in case it starts working");
        }

        WriteObject(new TeamsCardButton {
            Type = Type,
            Title = Title,
            Value = Value,
            Image = Image
        });
    }
}
