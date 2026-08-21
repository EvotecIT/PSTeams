using System.Collections;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates or sends a Teams ListCard payload.
/// </summary>
[Cmdlet(VerbsCommon.New, "CardList", SupportsShouldProcess = true)]
[OutputType(typeof(string))]
public sealed class CmdletNewCardList : AsyncPSCmdlet {
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock Content { get; set; } = null!;

    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    [Parameter(Mandatory = false)]
    public Uri? Uri { get; set; }

    /// <summary>
    /// Gets or sets the HTTP proxy used when the card is sent.
    /// </summary>
    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    protected override async Task ProcessRecordAsync() {
        var card = new TeamsListCard {
            Title = Title
        };

        foreach (var item in Content.Invoke()) {
            ApplyItem(card, item is PSObject psObject ? psObject.BaseObject : item);
        }

        var body = TeamsWrapperCardRenderer.Render(card);
        if (Uri is null) {
            WriteObject(body);
            return;
        }

        if (!ShouldProcess(Uri.Host, "Send Teams ListCard using IncomingWebhook")) {
            return;
        }

        await SendAttachmentBodyAsync(body, Uri);
    }

    private void ApplyItem(TeamsListCard card, object? value) {
        if (value is null) {
            return;
        }

        if (value is TeamsCardButton button) {
            if (card.Buttons.Count < 6) {
                card.Buttons.Add(button);
            } else {
                WriteWarning("New-CardList - List Cards support only up to 6 buttons.");
            }

            return;
        }

        if (value is TeamsListCardItem item) {
            card.Items.Add(item);
            return;
        }

        if (value is IDictionary dictionary) {
            if (TryCreateListItem(dictionary, out var fallbackItem)) {
                ApplyItem(card, fallbackItem);
                return;
            }

            if (CmdletNewHeroCard.TryCreateCardButton(dictionary, out var fallbackButton)) {
                ApplyItem(card, fallbackButton);
            }
        }
    }

    private async Task SendAttachmentBodyAsync(string attachmentBody, Uri uri) {
        using var clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(Proxy);
        var target = TeamsMessageTarget.ForIncomingWebhook(uri);
        var wrappedBody = TeamsWrapperCardRenderer.WrapAsMessage(attachmentBody);
        var result = await clientLease.Client.SendJsonAsync(wrappedBody, target, CancelToken);

        if (!result.IsSuccessStatusCode) {
            WriteError(TeamsPowerShellDeliverySupport.CreateDeliveryFailureError(result, "New-CardList"));
        }
    }

    private static bool TryCreateListItem(IDictionary dictionary, out TeamsListCardItem item) {
        if (dictionary.Contains("type")) {
            TeamsCardButtonActionType? tapType = null;
            string? tapValue = null;
            string? tapAction = null;

            if (dictionary.Contains("tap") && dictionary["tap"] is IDictionary tapDictionary) {
                tapType = ParseTapType(tapDictionary["type"]?.ToString());
                var combinedValue = tapDictionary["value"]?.ToString();
                if (!string.IsNullOrWhiteSpace(combinedValue)) {
                    var parts = combinedValue!.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2) {
                        tapAction = parts[0];
                        tapValue = parts[1];
                    } else {
                        tapValue = combinedValue;
                    }
                }
            }

            item = new TeamsListCardItem {
                Kind = ParseItemKind(dictionary["type"]?.ToString()),
                Icon = dictionary.Contains("icon") ? dictionary["icon"]?.ToString() : null,
                Title = dictionary.Contains("title") ? dictionary["title"]?.ToString() : null,
                SubTitle = dictionary.Contains("subtitle") ? dictionary["subtitle"]?.ToString() : null,
                TapAction = tapAction,
                TapType = tapType,
                TapValue = tapValue
            };
            return true;
        }

        item = null!;
        return false;
    }

    private static TeamsCardButtonActionType? ParseTapType(string? type) {
        if (string.IsNullOrWhiteSpace(type)) {
            return null;
        }

        return type switch {
            "imBack" => TeamsCardButtonActionType.ImBack,
            "file" => TeamsCardButtonActionType.File,
            _ => TeamsCardButtonActionType.OpenUrl
        };
    }

    private static TeamsListCardItemKind ParseItemKind(string? type) {
        return type switch {
            "file" => TeamsListCardItemKind.File,
            "section" => TeamsListCardItemKind.Section,
            "person" => TeamsListCardItemKind.Person,
            _ => TeamsListCardItemKind.ResultItem
        };
    }
}
