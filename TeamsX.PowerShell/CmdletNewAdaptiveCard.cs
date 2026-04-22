using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive card message backed by the TeamsX model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveCard", SupportsShouldProcess = true)]
[OutputType(typeof(string))]
public sealed class CmdletNewAdaptiveCard : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Body { get; set; }

    [Parameter(Mandatory = false)]
    public ScriptBlock? Action { get; set; }

    [Alias("TeamsID", "Url")]
    [Parameter(Mandatory = false)]
    public Uri? Uri { get; set; }

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

    protected override void ProcessRecord() {
        var card = new TeamsAdaptiveCard {
            FallbackText = FallBackText,
            MinimumHeight = MinimumHeight > 0 ? $"{MinimumHeight}px" : null,
            Speak = Speak,
            Language = Language,
            VerticalContentAlignment = VerticalContentAlignment,
            BackgroundImage = BuildBackgroundImage(),
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

        var client = TeamsPowerShellDeliverySupport.CreateClient(null);
        var target = TeamsMessageTarget.ForIncomingWebhook(Uri);
        var result = client.SendJsonAsync(jsonBody, target).GetAwaiter().GetResult();

        TeamsPowerShellDeliverySupport.WriteDeliveryIssue(this, result, "New-AdaptiveCard");

        if (ReturnJson.IsPresent) {
            WriteObject(jsonBody);
        }
    }

    private Dictionary<string, object?>? BuildBackgroundImage() {
        if (string.IsNullOrWhiteSpace(BackgroundUrl) &&
            string.IsNullOrWhiteSpace(BackgroundFillMode) &&
            string.IsNullOrWhiteSpace(BackgroundHorizontalAlignment) &&
            string.IsNullOrWhiteSpace(BackgroundVerticalAlignment)) {
            return null;
        }

        var backgroundImage = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(BackgroundFillMode)) {
            backgroundImage["fillMode"] = BackgroundFillMode;
        }

        if (!string.IsNullOrWhiteSpace(BackgroundHorizontalAlignment)) {
            backgroundImage["horizontalAlignment"] = BackgroundHorizontalAlignment;
        }

        if (!string.IsNullOrWhiteSpace(BackgroundVerticalAlignment)) {
            backgroundImage["verticalAlignment"] = BackgroundVerticalAlignment;
        }

        if (!string.IsNullOrWhiteSpace(BackgroundUrl)) {
            backgroundImage["url"] = BackgroundUrl;
        }

        return backgroundImage;
    }
}
