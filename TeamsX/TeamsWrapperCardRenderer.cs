namespace TeamsX;

/// <summary>
/// Renders Teams HeroCard, ThumbnailCard, and ListCard payloads.
/// </summary>
public static class TeamsWrapperCardRenderer {
    public static string Render(TeamsHeroCard card) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        var payload = new Dictionary<string, object?> {
            ["contentType"] = "application/vnd.microsoft.card.hero",
            ["content"] = new Dictionary<string, object?> {
                ["title"] = EmptyToNull(card.Title),
                ["subTitle"] = EmptyToNull(card.SubTitle),
                ["text"] = EmptyToNull(card.Text),
                ["images"] = card.Images.Count == 0 ? null : card.Images.Select(RenderImage).ToArray(),
                ["buttons"] = card.Buttons.Count == 0 ? null : card.Buttons.Select(RenderButton).ToArray()
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    public static string Render(TeamsThumbnailCard card) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        var payload = new Dictionary<string, object?> {
            ["contentType"] = "application/vnd.microsoft.card.thumbnail",
            ["content"] = new Dictionary<string, object?> {
                ["title"] = EmptyToNull(card.Title),
                ["subTitle"] = EmptyToNull(card.SubTitle),
                ["text"] = EmptyToNull(card.Text),
                ["images"] = card.Images.Count == 0 ? null : card.Images.Select(RenderImage).ToArray(),
                ["buttons"] = card.Buttons.Count == 0 ? null : card.Buttons.Select(RenderButton).ToArray()
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    public static string Render(TeamsListCard card) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        var payload = new Dictionary<string, object?> {
            ["contentType"] = "application/vnd.microsoft.teams.card.list",
            ["content"] = new Dictionary<string, object?> {
                ["title"] = EmptyToNull(card.Title),
                ["items"] = card.Items.Count == 0 ? null : card.Items.Select(RenderListItem).ToArray(),
                ["buttons"] = card.Buttons.Count == 0 ? null : card.Buttons.Select(RenderButton).ToArray()
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    public static string WrapAsMessage(string attachmentBodyJson) {
        if (attachmentBodyJson is null) {
            throw new ArgumentNullException(nameof(attachmentBodyJson));
        }

        var trimmedBody = string.IsNullOrWhiteSpace(attachmentBodyJson)
            ? "null"
            : attachmentBodyJson.Trim();

        return $"{{\"type\":\"message\",\"attachments\":[{trimmedBody}]}}";
    }

    private static Dictionary<string, object?> RenderImage(TeamsCardImage image) {
        return new Dictionary<string, object?> {
            ["url"] = EmptyToNull(image.Url),
            ["alt"] = EmptyToNull(image.Alt)
        };
    }

    private static Dictionary<string, object?> RenderButton(TeamsCardButton button) {
        return new Dictionary<string, object?> {
            ["type"] = RenderActionType(button.Type),
            ["title"] = EmptyToNull(button.Title),
            ["value"] = EmptyToNull(button.Value),
            ["image"] = EmptyToNull(button.Image)
        };
    }

    private static Dictionary<string, object?> RenderListItem(TeamsListCardItem item) {
        var tapValue = BuildTapValue(item);

        return new Dictionary<string, object?> {
            ["type"] = RenderItemKind(item.Kind),
            ["id"] = string.IsNullOrWhiteSpace(item.TapAction) ? null : EmptyToNull(item.TapValue),
            ["title"] = EmptyToNull(item.Title),
            ["subtitle"] = EmptyToNull(item.SubTitle),
            ["icon"] = EmptyToNull(item.Icon),
            ["tap"] = item.TapType.HasValue
                ? new Dictionary<string, object?> {
                    ["type"] = RenderActionType(item.TapType.Value),
                    ["value"] = EmptyToNull(tapValue)
                }
                : null
        };
    }

    private static string? BuildTapValue(TeamsListCardItem item) {
        var combined = $"{item.TapAction} {item.TapValue}".Trim();
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private static string RenderActionType(TeamsCardButtonActionType actionType) {
        return actionType switch {
            TeamsCardButtonActionType.ImBack => "imBack",
            TeamsCardButtonActionType.OpenUrl => "openUrl",
            TeamsCardButtonActionType.File => "file",
            _ => throw new NotSupportedException($"Wrapper-card action '{actionType}' is not supported.")
        };
    }

    private static string RenderItemKind(TeamsListCardItemKind kind) {
        return kind switch {
            TeamsListCardItemKind.File => "file",
            TeamsListCardItemKind.ResultItem => "resultItem",
            TeamsListCardItemKind.Section => "section",
            TeamsListCardItemKind.Person => "person",
            _ => throw new NotSupportedException($"List-card item kind '{kind}' is not supported.")
        };
    }

    private static string? EmptyToNull(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
