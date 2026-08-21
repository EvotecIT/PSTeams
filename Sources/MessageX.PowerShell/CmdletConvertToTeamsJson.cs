using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

[Cmdlet(VerbsData.ConvertTo, "TeamsJson")]
[OutputType(typeof(string))]
public sealed class CmdletConvertToTeamsJson : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public object InputObject { get; set; } = null!;

    protected override void ProcessRecord() {
        switch (InputObject) {
            case TeamsMessageRequest message:
                WriteObject(WebhookMessageRenderer.Render(message));
                return;
            case TeamsHeroCard heroCard:
                WriteObject(TeamsWrapperCardRenderer.Render(heroCard));
                return;
            case TeamsThumbnailCard thumbnailCard:
                WriteObject(TeamsWrapperCardRenderer.Render(thumbnailCard));
                return;
            case TeamsListCard listCard:
                WriteObject(TeamsWrapperCardRenderer.Render(listCard));
                return;
            case PSObject { BaseObject: TeamsMessageRequest message }:
                WriteObject(WebhookMessageRenderer.Render(message));
                return;
            case PSObject { BaseObject: TeamsHeroCard heroCard }:
                WriteObject(TeamsWrapperCardRenderer.Render(heroCard));
                return;
            case PSObject { BaseObject: TeamsThumbnailCard thumbnailCard }:
                WriteObject(TeamsWrapperCardRenderer.Render(thumbnailCard));
                return;
            case PSObject { BaseObject: TeamsListCard listCard }:
                WriteObject(TeamsWrapperCardRenderer.Render(listCard));
                return;
            default:
                ThrowTerminatingError(new ErrorRecord(
                    new PSArgumentException("ConvertTo-TeamsJson supports TeamsMessageRequest, TeamsHeroCard, TeamsThumbnailCard, and TeamsListCard inputs."),
                    "UnsupportedTeamsJsonInput",
                    ErrorCategory.InvalidArgument,
                    InputObject));
                return;
        }
    }
}
