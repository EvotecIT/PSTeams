using System.Collections;
using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates or sends a Teams ThumbnailCard payload.
/// </summary>
[Cmdlet(VerbsCommon.New, "ThumbnailCard", SupportsShouldProcess = true)]
[OutputType(typeof(string))]
public sealed class CmdletNewThumbnailCard : PSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock Content { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public string? SubTitle { get; set; }

    [Parameter(Mandatory = false)]
    public string? Text { get; set; }

    [Parameter(Mandatory = false)]
    public Uri? Uri { get; set; }

    protected override void ProcessRecord() {
        var card = new TeamsThumbnailCard {
            Title = Title,
            SubTitle = SubTitle,
            Text = Text
        };

        foreach (var item in Content.Invoke()) {
            ApplyItem(card, item is PSObject psObject ? psObject.BaseObject : item);
        }

        var body = TeamsWrapperCardRenderer.Render(card);
        if (Uri is null) {
            WriteObject(body);
            return;
        }

        if (!ShouldProcess(Uri.Host, "Send Teams ThumbnailCard using IncomingWebhook")) {
            return;
        }

        SendAttachmentBody(body, Uri);
    }

    private void ApplyItem(TeamsThumbnailCard card, object? value) {
        if (value is null) {
            return;
        }

        if (value is TeamsCardButton button) {
            if (card.Buttons.Count < 6) {
                card.Buttons.Add(button);
            } else {
                WriteWarning("New-ThumbnailCard - Thumbnail Card support only up to 6 buttons.");
            }

            return;
        }

        if (value is TeamsCardImage image) {
            if (card.Images.Count < 1) {
                card.Images.Add(image);
            } else {
                WriteWarning("New-ThumbnailCard - Thumbnail Card support only 1 image.");
            }

            return;
        }

        if (CmdletNewHeroCard.TryCreateCardImage(value, out var mappedImage)) {
            ApplyItem(card, mappedImage);
            return;
        }

        if (value is IDictionary dictionary) {
            if (CmdletNewHeroCard.TryCreateCardButton(dictionary, out var fallbackButton)) {
                ApplyItem(card, fallbackButton);
                return;
            }

            if (CmdletNewHeroCard.TryCreateCardImage(dictionary, out var fallbackImage)) {
                ApplyItem(card, fallbackImage);
            }
        }
    }

    private void SendAttachmentBody(string attachmentBody, Uri uri) {
        var client = TeamsPowerShellDeliverySupport.CreateClient(null);
        var target = TeamsMessageTarget.ForIncomingWebhook(uri);
        var wrappedBody = TeamsWrapperCardRenderer.WrapAsMessage(attachmentBody);
        var result = client.SendJsonAsync(wrappedBody, target).GetAwaiter().GetResult();

        TeamsPowerShellDeliverySupport.WriteDeliveryIssue(this, result, "New-ThumbnailCard");
    }
}
