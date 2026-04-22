namespace TeamsX;

public static class WebhookMessageRenderer {
    public static string Render(TeamsMessageRequest request) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.AdaptiveCard is not null) {
            return RenderAdaptiveCardMessage(request);
        }

        var payload = new Dictionary<string, object?> {
            ["summary"] = EmptyToNull(request.EffectiveSummary),
            ["title"] = EmptyToNull(request.Title),
            ["text"] = EmptyToNull(request.Text)
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    private static string RenderAdaptiveCardMessage(TeamsMessageRequest request) {
        var payload = new Dictionary<string, object?> {
            ["type"] = "message",
            ["summary"] = EmptyToNull(request.EffectiveSummary),
            ["attachments"] = new[] {
                new Dictionary<string, object?> {
                    ["contentType"] = "application/vnd.microsoft.card.adaptive",
                    ["contentUrl"] = null,
                    ["content"] = RenderAdaptiveCard(request.AdaptiveCard!)
                }
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    internal static Dictionary<string, object?> RenderAdaptiveCard(TeamsAdaptiveCard card) {
        var content = new Dictionary<string, object?> {
            ["$schema"] = card.Schema,
            ["type"] = card.Type,
            ["version"] = card.Version,
            ["body"] = card.Body.Select(RenderAdaptiveElement).ToArray(),
            ["actions"] = card.Actions.Count == 0 ? null : card.Actions.Select(RenderAdaptiveAction).ToArray(),
            ["msteams"] = card.Mentions.Count == 0
                ? null
                : new Dictionary<string, object?> {
                    ["entities"] = card.Mentions.Select(RenderAdaptiveMention).ToArray()
                }
        };

        return content;
    }

    private static Dictionary<string, object?> RenderAdaptiveMention(TeamsAdaptiveMention mention) {
        return new Dictionary<string, object?> {
            ["type"] = mention.Type,
            ["text"] = mention.Text,
            ["mentioned"] = new Dictionary<string, object?> {
                ["id"] = mention.Mentioned.Id,
                ["name"] = EmptyToNull(mention.Mentioned.Name)
            }
        };
    }

    private static Dictionary<string, object?> RenderAdaptiveElement(TeamsAdaptiveCardElement element) {
        if (element is TeamsAdaptiveTextBlock textBlock) {
            return new Dictionary<string, object?> {
                ["type"] = textBlock.Type,
                ["text"] = textBlock.Text,
                ["wrap"] = textBlock.Wrap,
                ["size"] = EmptyToNull(textBlock.Size),
                ["weight"] = EmptyToNull(textBlock.Weight),
                ["color"] = EmptyToNull(textBlock.Color)
            };
        }

        if (element is TeamsAdaptiveRichTextBlock richTextBlock) {
            return new Dictionary<string, object?> {
                ["type"] = richTextBlock.Type,
                ["inlines"] = richTextBlock.Inlines.Select(inline => new Dictionary<string, object?> {
                    ["type"] = "TextRun",
                    ["text"] = inline.Text,
                    ["color"] = EmptyToNull(inline.Color),
                    ["subtle"] = inline.Subtle,
                    ["size"] = EmptyToNull(inline.Size),
                    ["weight"] = EmptyToNull(inline.Weight),
                    ["highlight"] = inline.Highlight,
                    ["italic"] = inline.Italic,
                    ["strikethrough"] = inline.StrikeThrough,
                    ["fontType"] = EmptyToNull(inline.FontType)
                }).ToArray()
            };
        }

        if (element is TeamsAdaptiveFactSet factSet) {
            return new Dictionary<string, object?> {
                ["type"] = factSet.Type,
                ["facts"] = factSet.Facts.Select(fact => new Dictionary<string, object?> {
                    ["title"] = fact.Title,
                    ["value"] = fact.Value
                }).ToArray()
            };
        }

        if (element is TeamsAdaptiveImage image) {
            return new Dictionary<string, object?> {
                ["type"] = image.Type,
                ["url"] = image.Url,
                ["altText"] = EmptyToNull(image.AltText),
                ["size"] = EmptyToNull(image.Size)
            };
        }

        if (element is TeamsAdaptiveMedia media) {
            return new Dictionary<string, object?> {
                ["type"] = media.Type,
                ["poster"] = EmptyToNull(media.Poster),
                ["altText"] = EmptyToNull(media.AltText),
                ["id"] = EmptyToNull(media.Id),
                ["horizontalAlignment"] = EmptyToNull(media.HorizontalAlignment),
                ["height"] = EmptyToNull(media.Height),
                ["spacing"] = EmptyToNull(media.Spacing),
                ["separator"] = media.Separator,
                ["isVisible"] = media.IsVisible,
                ["sources"] = media.Sources.Select(source => new Dictionary<string, object?> {
                    ["mimeType"] = EmptyToNull(source.MimeType),
                    ["url"] = EmptyToNull(source.Url)
                }).ToArray()
            };
        }

        if (element is TeamsAdaptiveImageSet imageSet) {
            return new Dictionary<string, object?> {
                ["type"] = imageSet.Type,
                ["imageSize"] = EmptyToNull(imageSet.ImageSize),
                ["images"] = imageSet.Images.Select(image => (object?)RenderAdaptiveElement(image)).ToArray()
            };
        }

        if (element is TeamsAdaptiveContainer container) {
            return new Dictionary<string, object?> {
                ["type"] = container.Type,
                ["items"] = container.Items.Select(RenderAdaptiveElement).ToArray()
            };
        }

        if (element is TeamsAdaptiveColumn column) {
            return new Dictionary<string, object?> {
                ["type"] = column.Type,
                ["width"] = EmptyToNull(column.Width),
                ["items"] = column.Items.Select(RenderAdaptiveElement).ToArray()
            };
        }

        if (element is TeamsAdaptiveColumnSet columnSet) {
            return new Dictionary<string, object?> {
                ["type"] = columnSet.Type,
                ["columns"] = columnSet.Columns.Select(column => (object?)RenderAdaptiveElement(column)).ToArray()
            };
        }

        if (element is TeamsAdaptiveActionSet actionSet) {
            return new Dictionary<string, object?> {
                ["type"] = actionSet.Type,
                ["actions"] = actionSet.Actions.Select(RenderAdaptiveAction).ToArray()
            };
        }

        throw new NotSupportedException($"Adaptive element '{element.GetType().Name}' is not supported by the webhook renderer yet.");
    }

    private static Dictionary<string, object?> RenderAdaptiveAction(TeamsAdaptiveAction action) {
        if (action is TeamsAdaptiveOpenUrlAction openUrlAction) {
            return new Dictionary<string, object?> {
                ["type"] = openUrlAction.Type,
                ["title"] = EmptyToNull(openUrlAction.Title),
                ["url"] = openUrlAction.Url
            };
        }

        if (action is TeamsAdaptiveToggleVisibilityAction toggleVisibilityAction) {
            return new Dictionary<string, object?> {
                ["type"] = toggleVisibilityAction.Type,
                ["title"] = EmptyToNull(toggleVisibilityAction.Title),
                ["targetElements"] = toggleVisibilityAction.TargetElements.ToArray()
            };
        }

        throw new NotSupportedException($"Adaptive action '{action.GetType().Name}' is not supported by the webhook renderer yet.");
    }

    private static string? EmptyToNull(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
