using System.Collections;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates or sends a Teams HeroCard payload.
/// </summary>
[Cmdlet(VerbsCommon.New, "HeroCard", SupportsShouldProcess = true)]
[OutputType(typeof(string))]
public sealed class CmdletNewHeroCard : AsyncPSCmdlet {
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

    /// <summary>
    /// Gets or sets the HTTP proxy used when the card is sent.
    /// </summary>
    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    protected override async Task ProcessRecordAsync() {
        var card = new TeamsHeroCard {
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

        if (!ShouldProcess(Uri.Host, "Send Teams HeroCard using IncomingWebhook")) {
            return;
        }

        await SendAttachmentBodyAsync(body, Uri);
    }

    private void ApplyItem(TeamsHeroCard card, object? value) {
        if (value is null) {
            return;
        }

        if (value is TeamsCardButton button) {
            if (card.Buttons.Count < 3) {
                card.Buttons.Add(button);
            } else {
                WriteWarning("New-HeroCard - Herd Card support only up to 3 buttons.");
            }

            return;
        }

        if (value is TeamsCardImage image) {
            if (card.Images.Count < 2) {
                card.Images.Add(image);
            } else {
                WriteWarning("New-HeroCard - Herd Card support only 1 image.");
            }

            return;
        }

        if (TryCreateCardImage(value, out var mappedImage)) {
            ApplyItem(card, mappedImage);
            return;
        }

        if (value is IDictionary dictionary) {
            if (TryCreateCardButton(dictionary, out var fallbackButton)) {
                ApplyItem(card, fallbackButton);
                return;
            }

            if (TryCreateCardImage(dictionary, out var fallbackImage)) {
                ApplyItem(card, fallbackImage);
            }
        }
    }

    private async Task SendAttachmentBodyAsync(string attachmentBody, Uri uri) {
        using var clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(Proxy);
        var target = TeamsMessageTarget.ForIncomingWebhook(uri);
        var wrappedBody = TeamsWrapperCardRenderer.WrapAsMessage(attachmentBody);
        var result = await clientLease.Client.SendJsonAsync(wrappedBody, target, CancelToken);

        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "New-HeroCard"));
        }
    }

    internal static bool TryCreateCardButton(IDictionary dictionary, out TeamsCardButton button) {
        if (dictionary.Contains("title") || dictionary.Contains("value")) {
            button = new TeamsCardButton {
                Type = ParseButtonType(dictionary["type"]?.ToString()),
                Title = dictionary["title"]?.ToString(),
                Value = dictionary["value"]?.ToString(),
                Image = dictionary["image"]?.ToString()
            };
            return true;
        }

        button = null!;
        return false;
    }

    internal static bool TryCreateCardImage(object? value, out TeamsCardImage image) {
        if (value is TeamsAdaptiveImage adaptiveImage) {
            image = new TeamsCardImage {
                Url = adaptiveImage.Url,
                Alt = adaptiveImage.AltText
            };
            return !string.IsNullOrWhiteSpace(image.Url);
        }

        if (value is TeamsCardImage teamsCardImage) {
            image = teamsCardImage;
            return !string.IsNullOrWhiteSpace(image.Url);
        }

        image = null!;
        return false;
    }

    internal static bool TryCreateCardImage(IDictionary dictionary, out TeamsCardImage image) {
        if (dictionary.Contains("url")) {
            image = new TeamsCardImage {
                Url = dictionary["url"]?.ToString(),
                Alt = dictionary.Contains("alt") ? dictionary["alt"]?.ToString() : null
            };
            return true;
        }

        image = null!;
        return false;
    }

    private static TeamsCardButtonActionType ParseButtonType(string? type) {
        return type switch {
            "imBack" => TeamsCardButtonActionType.ImBack,
            "file" => TeamsCardButtonActionType.File,
            _ => TeamsCardButtonActionType.OpenUrl
        };
    }
}
