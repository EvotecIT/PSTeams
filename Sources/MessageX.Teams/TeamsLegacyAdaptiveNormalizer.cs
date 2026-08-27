using System.Collections;

namespace MessageX.Teams;

/// <summary>
/// Converts MessageX.Teams adaptive object graphs into legacy dictionary-based payloads that the script module can serialize.
/// </summary>
public static class TeamsLegacyAdaptiveNormalizer {
    public static object? Normalize(object? value) {
        if (value is null) {
            return null;
        }

        var psObjectType = value.GetType();
        if (string.Equals(psObjectType.FullName, "System.Management.Automation.PSObject", StringComparison.Ordinal)) {
            var baseObject = psObjectType.GetProperty("BaseObject")?.GetValue(value);
            return Normalize(baseObject);
        }

        if (value is IDictionary dictionary) {
            var normalized = new Dictionary<string, object?>();
            foreach (DictionaryEntry entry in dictionary) {
                normalized[entry.Key?.ToString() ?? string.Empty] = Normalize(entry.Value);
            }

            return normalized;
        }

        if (value is IEnumerable enumerable && value is not string) {
            var items = new List<object?>();
            foreach (var item in enumerable) {
                items.Add(Normalize(item));
            }

            return items;
        }

        if (value is TeamsAdaptiveCard card) {
            TeamsAdaptiveCardValidation.Validate(card);
            return new Dictionary<string, object?> {
                ["$schema"] = card.Schema,
                ["type"] = card.Type,
                ["version"] = card.Version,
                ["body"] = Normalize(card.Body),
                ["actions"] = Normalize(card.Actions),
                ["fallbackText"] = EmptyToNull(card.FallbackText),
                ["minHeight"] = EmptyToNull(card.MinimumHeight),
                ["speak"] = EmptyToNull(card.Speak),
                ["lang"] = EmptyToNull(card.Language),
                ["verticalContentAlignment"] = EmptyToNull(card.VerticalContentAlignment),
                ["backgroundImage"] = Normalize(card.BackgroundImage),
                ["selectAction"] = Normalize(card.SelectAction),
                ["refresh"] = Normalize(card.Refresh),
                ["msteams"] = BuildMsTeams(card)
            };
        }

        if (value is TeamsAdaptiveRefresh refresh) {
            return new Dictionary<string, object?> {
                ["action"] = Normalize(refresh.Action),
                ["userIds"] = refresh.UserIds.Count == 0 ? null : Normalize(refresh.UserIds)
            };
        }

        if (value is TeamsAdaptiveBackgroundImage backgroundImage) {
            var payload = new Dictionary<string, object?>();
            AddNonEmpty(payload, "fillMode", backgroundImage.FillMode);
            AddNonEmpty(payload, "horizontalAlignment", backgroundImage.HorizontalAlignment);
            AddNonEmpty(payload, "verticalAlignment", backgroundImage.VerticalAlignment);
            AddNonEmpty(payload, "url", backgroundImage.Url);
            return payload;
        }

        if (value is TeamsAdaptiveTextBlock textBlock) {
            return new Dictionary<string, object?> {
                ["type"] = textBlock.Type,
                ["text"] = textBlock.Text,
                ["id"] = EmptyToNull(textBlock.Id),
                ["spacing"] = EmptyToNull(textBlock.Spacing),
                ["horizontalAlignment"] = EmptyToNull(textBlock.HorizontalAlignment),
                ["size"] = EmptyToNull(textBlock.Size),
                ["weight"] = EmptyToNull(textBlock.Weight),
                ["color"] = EmptyToNull(textBlock.Color),
                ["height"] = EmptyToNull(textBlock.Height),
                ["fontType"] = EmptyToNull(textBlock.FontType),
                ["highlight"] = textBlock.Highlight,
                ["italic"] = textBlock.Italic,
                ["strikeThrough"] = textBlock.StrikeThrough,
                ["maxLines"] = textBlock.MaximumLines,
                ["separator"] = textBlock.Separator,
                ["wrap"] = textBlock.Wrap,
                ["isSubtle"] = textBlock.Subtle,
                ["isVisible"] = textBlock.IsVisible,
            };
        }

        if (value is TeamsAdaptiveImage image) {
            return new Dictionary<string, object?> {
                ["type"] = image.Type,
                ["id"] = EmptyToNull(image.Id),
                ["url"] = image.Url,
                ["size"] = EmptyToNull(image.Size),
                ["altText"] = EmptyToNull(image.AltText),
                ["style"] = EmptyToNull(image.Style),
                ["horizontalAlignment"] = EmptyToNull(image.HorizontalAlignment),
                ["height"] = EmptyToNull(image.Height),
                ["width"] = EmptyToNull(image.Width),
                ["spacing"] = EmptyToNull(image.Spacing),
                ["backgroundColor"] = EmptyToNull(image.BackgroundColor),
                ["separator"] = image.Separator,
                ["isVisible"] = image.IsVisible,
                ["selectAction"] = Normalize(image.SelectAction)
            };
        }

        if (value is TeamsAdaptiveMedia media) {
            return new Dictionary<string, object?> {
                ["type"] = media.Type,
                ["poster"] = EmptyToNull(media.Poster),
                ["id"] = EmptyToNull(media.Id),
                ["altText"] = EmptyToNull(media.AltText),
                ["horizontalAlignment"] = EmptyToNull(media.HorizontalAlignment),
                ["height"] = EmptyToNull(media.Height),
                ["spacing"] = EmptyToNull(media.Spacing),
                ["separator"] = media.Separator,
                ["isVisible"] = media.IsVisible,
                ["sources"] = Normalize(media.Sources)
            };
        }

        if (value is TeamsAdaptiveMediaSource mediaSource) {
            return new Dictionary<string, object?> {
                ["mimeType"] = EmptyToNull(mediaSource.MimeType),
                ["url"] = EmptyToNull(mediaSource.Url)
            };
        }

        if (value is TeamsAdaptiveImageSet imageSet) {
            return new Dictionary<string, object?> {
                ["type"] = imageSet.Type,
                ["id"] = EmptyToNull(imageSet.Id),
                ["imageSize"] = EmptyToNull(imageSet.ImageSize),
                ["horizontalAlignment"] = EmptyToNull(imageSet.HorizontalAlignment),
                ["height"] = EmptyToNull(imageSet.Height),
                ["spacing"] = EmptyToNull(imageSet.Spacing),
                ["separator"] = imageSet.Separator,
                ["isVisible"] = imageSet.IsVisible,
                ["images"] = Normalize(imageSet.Images)
            };
        }

        if (value is TeamsAdaptiveFactSet factSet) {
            return new Dictionary<string, object?> {
                ["type"] = factSet.Type,
                ["height"] = EmptyToNull(factSet.Height),
                ["spacing"] = EmptyToNull(factSet.Spacing),
                ["separator"] = factSet.Separator,
                ["facts"] = Normalize(factSet.Facts)
            };
        }

        if (value is TeamsAdaptiveFact fact) {
            return new Dictionary<string, object?> {
                ["title"] = EmptyToNull(fact.Title),
                ["value"] = EmptyToNull(fact.Value)
            };
        }

        if (value is TeamsAdaptiveContainer container) {
            return new Dictionary<string, object?> {
                ["type"] = container.Type,
                ["id"] = EmptyToNull(container.Id),
                ["style"] = EmptyToNull(container.Style),
                ["verticalContentAlignment"] = EmptyToNull(container.VerticalContentAlignment),
                ["horizontalAlignment"] = EmptyToNull(container.HorizontalAlignment),
                ["height"] = EmptyToNull(container.Height),
                ["spacing"] = EmptyToNull(container.Spacing),
                ["bleed"] = container.Bleed,
                ["minHeight"] = EmptyToNull(container.MinimumHeight),
                ["separator"] = container.Separator,
                ["isVisible"] = container.IsVisible,
                ["backgroundImage"] = Normalize(container.BackgroundImage),
                ["selectAction"] = Normalize(container.SelectAction),
                ["items"] = Normalize(container.Items)
            };
        }

        if (value is TeamsAdaptiveColumn column) {
            return new Dictionary<string, object?> {
                ["type"] = column.Type,
                ["width"] = EmptyToNull(column.Width),
                ["height"] = EmptyToNull(column.Height),
                ["minHeight"] = EmptyToNull(column.MinimumHeight),
                ["horizontalAlignment"] = EmptyToNull(column.HorizontalAlignment),
                ["verticalContentAlignment"] = EmptyToNull(column.VerticalContentAlignment),
                ["spacing"] = EmptyToNull(column.Spacing),
                ["style"] = EmptyToNull(column.Style),
                ["isVisible"] = column.IsVisible,
                ["separator"] = column.Separator,
                ["selectAction"] = Normalize(column.SelectAction),
                ["items"] = Normalize(column.Items)
            };
        }

        if (value is TeamsAdaptiveColumnSet columnSet) {
            return new Dictionary<string, object?> {
                ["type"] = columnSet.Type,
                ["style"] = EmptyToNull(columnSet.Style),
                ["minHeight"] = EmptyToNull(columnSet.MinimumHeight),
                ["bleed"] = columnSet.Bleed,
                ["horizontalAlignment"] = EmptyToNull(columnSet.HorizontalAlignment),
                ["height"] = EmptyToNull(columnSet.Height),
                ["spacing"] = EmptyToNull(columnSet.Spacing),
                ["separator"] = columnSet.Separator,
                ["columns"] = Normalize(columnSet.Columns)
            };
        }

        if (value is TeamsAdaptiveRichTextBlock richTextBlock) {
            return new Dictionary<string, object?> {
                ["type"] = richTextBlock.Type,
                ["id"] = EmptyToNull(richTextBlock.Id),
                ["horizontalAlignment"] = EmptyToNull(richTextBlock.HorizontalAlignment),
                ["height"] = EmptyToNull(richTextBlock.Height),
                ["spacing"] = EmptyToNull(richTextBlock.Spacing),
                ["separator"] = richTextBlock.Separator,
                ["isVisible"] = richTextBlock.IsVisible,
                ["inlines"] = Normalize(richTextBlock.Inlines)
            };
        }

        if (value is TeamsAdaptiveTextRun textRun) {
            return new Dictionary<string, object?> {
                ["type"] = "TextRun",
                ["text"] = textRun.Text,
                ["color"] = EmptyToNull(textRun.Color),
                ["subtle"] = textRun.Subtle,
                ["size"] = EmptyToNull(textRun.Size),
                ["weight"] = EmptyToNull(textRun.Weight),
                ["highlight"] = textRun.Highlight,
                ["italic"] = textRun.Italic,
                ["strikethrough"] = textRun.StrikeThrough,
                ["fontType"] = EmptyToNull(textRun.FontType)
            };
        }

        if (value is TeamsAdaptiveActionSet actionSet) {
            return new Dictionary<string, object?> {
                ["type"] = actionSet.Type,
                ["actions"] = Normalize(actionSet.Actions)
            };
        }

        if (value is TeamsAdaptiveOpenUrlAction openUrlAction) {
            return new Dictionary<string, object?> {
                ["type"] = openUrlAction.Type,
                ["id"] = EmptyToNull(openUrlAction.Id),
                ["title"] = EmptyToNull(openUrlAction.Title),
                ["url"] = EmptyToNull(openUrlAction.Url)
            };
        }

        if (value is TeamsAdaptiveToggleVisibilityAction toggleVisibilityAction) {
            return new Dictionary<string, object?> {
                ["type"] = toggleVisibilityAction.Type,
                ["id"] = EmptyToNull(toggleVisibilityAction.Id),
                ["title"] = EmptyToNull(toggleVisibilityAction.Title),
                ["targetElements"] = Normalize(toggleVisibilityAction.TargetElements)
            };
        }

        if (value is TeamsAdaptiveExecuteAction executeAction) {
            return new Dictionary<string, object?> {
                ["type"] = executeAction.Type,
                ["id"] = EmptyToNull(executeAction.Id),
                ["title"] = EmptyToNull(executeAction.Title),
                ["verb"] = executeAction.Verb,
                ["data"] = executeAction.Data,
                ["associatedInputs"] = executeAction.AssociatedInputs == TeamsAdaptiveAssociatedInputs.None
                    ? "none"
                    : "auto",
                ["fallback"] = Normalize(executeAction.Fallback)
            };
        }

        if (value is TeamsAdaptiveSubmitAction submitAction) {
            return new Dictionary<string, object?> {
                ["type"] = submitAction.Type,
                ["id"] = EmptyToNull(submitAction.Id),
                ["title"] = EmptyToNull(submitAction.Title),
                ["data"] = submitAction.Data
            };
        }

        if (value is TeamsAdaptiveShowCardAction showCardAction) {
            return new Dictionary<string, object?> {
                ["type"] = showCardAction.Type,
                ["id"] = EmptyToNull(showCardAction.Id),
                ["title"] = EmptyToNull(showCardAction.Title),
                ["card"] = Normalize(showCardAction.Card)
            };
        }

        if (value is TeamsAdaptiveMention mention) {
            return new Dictionary<string, object?> {
                ["type"] = mention.Type,
                ["text"] = mention.Text,
                ["mentioned"] = new Dictionary<string, object?> {
                    ["id"] = mention.Mentioned.Id,
                    ["name"] = EmptyToNull(mention.Mentioned.Name)
                }
            };
        }

        return value;
    }

    private static Dictionary<string, object?>? BuildMsTeams(TeamsAdaptiveCard card) {
        var payload = new Dictionary<string, object?>();
        if (card.AllowImageExpand is not null) {
            payload["allowExpand"] = card.AllowImageExpand;
        }

        if (card.FullWidth) {
            payload["width"] = "Full";
        }

        if (card.Mentions.Count > 0) {
            payload["entities"] = Normalize(card.Mentions);
        }

        return payload.Count == 0 ? null : payload;
    }

    private static string? EmptyToNull(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void AddNonEmpty(IDictionary<string, object?> dictionary, string key, string? value) {
        if (!string.IsNullOrWhiteSpace(value)) {
            dictionary[key] = value;
        }
    }
}
