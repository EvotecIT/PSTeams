using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive card message backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveCard", SupportsShouldProcess = true)]
[OutputType(typeof(string))]
public sealed class CmdletNewAdaptiveCard : AsyncPSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Body { get; set; }

    [Parameter(Mandatory = false)]
    public ScriptBlock? Action { get; set; }

    [Alias("TeamsID", "Url")]
    [Parameter(Mandatory = false)]
    public Uri? Uri { get; set; }

    /// <summary>
    /// Gets or sets the HTTP proxy used when the card is sent.
    /// </summary>
    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    [Parameter(Mandatory = false)]
    public string? FallBackText { get; set; }

    [Parameter(Mandatory = false)]
    public int MinimumHeight { get; set; }

    [Parameter(Mandatory = false)]
    public string? Speak { get; set; }

    [Parameter(Mandatory = false)]
    public string? Language { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("top", "center", "bottom")]
    public string? VerticalContentAlignment { get; set; }

    [Parameter(Mandatory = false)]
    public string? BackgroundUrl { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Cover", "RepeatHorizontally", "RepeatVertically", "Repeat")]
    public string? BackgroundFillMode { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("left", "center", "right")]
    public string? BackgroundHorizontalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("top", "center", "bottom")]
    public string? BackgroundVerticalAlignment { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Action.Submit", "Action.OpenUrl", "Action.ToggleVisibility")]
    public string? SelectAction { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionId { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionUrl { get; set; }

    [Parameter(Mandatory = false)]
    public string? SelectActionTitle { get; set; }

    [Parameter(Mandatory = false)]
    public string[]? SelectActionTargetElement { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter FullWidth { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter AllowImageExpand { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter ReturnJson { get; set; }

    protected override async Task ProcessRecordAsync() {
        var card = new TeamsAdaptiveCard {
            FallbackText = FallBackText,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Speak = Speak,
            Language = Language,
            VerticalContentAlignment = VerticalContentAlignment,
            BackgroundImage = TeamsAdaptiveBackgroundImageSupport.Create(
                BackgroundUrl,
                BackgroundFillMode,
                BackgroundHorizontalAlignment,
                BackgroundVerticalAlignment),
            SelectAction = TeamsAdaptiveActionSupport.CreateSelectAction(
                SelectAction,
                SelectActionId,
                SelectActionUrl,
                SelectActionTitle,
                SelectActionTargetElement),
            AllowImageExpand = AllowImageExpand.IsPresent ? true : null,
            FullWidth = FullWidth.IsPresent
        };

        if (Body is not null) {
            foreach (var item in Body.Invoke()) {
                var value = item is PSObject psObject ? psObject.BaseObject : item;
                if (value is TeamsAdaptiveMention mention) {
                    card.Mentions.Add(mention);
                    continue;
                }

                if (value is TeamsAdaptiveCardElement element) {
                    card.Body.Add(element);
                }
            }
        }

        if (Action is not null) {
            foreach (var item in Action.Invoke()) {
                var value = item is PSObject psObject ? psObject.BaseObject : item;
                if (value is TeamsAdaptiveAction adaptiveAction) {
                    card.Actions.Add(adaptiveAction);
                }
            }
        }

        var request = new TeamsMessageRequest {
            AdaptiveCard = card
        };
        var jsonBody = WebhookMessageRenderer.Render(request);

        if (Uri is null) {
            WriteObject(jsonBody);
            return;
        }

        if (!ShouldProcess(Uri.Host, "Send Teams adaptive card using IncomingWebhook")) {
            if (ReturnJson.IsPresent) {
                WriteObject(jsonBody);
            }

            return;
        }

        using var clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(Proxy);
        var target = TeamsMessageTarget.ForIncomingWebhook(Uri);
        var result = await clientLease.Client.SendJsonAsync(jsonBody, target, CancelToken);

        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "New-AdaptiveCard"));
        }

        if (ReturnJson.IsPresent) {
            WriteObject(jsonBody);
        }
    }

}
