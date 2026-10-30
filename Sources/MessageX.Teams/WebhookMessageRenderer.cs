namespace MessageX.Teams;

public static class WebhookMessageRenderer {
    public static string Render(TeamsMessageRequest request) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        return RenderAdaptiveCardMessage(request);
    }

    private static string RenderAdaptiveCardMessage(TeamsMessageRequest request) {
        var payload = new Dictionary<string, object?> {
            ["type"] = "message",
            ["summary"] = EmptyToNull(request.EffectiveSummary),
            ["attachments"] = new[] {
                new Dictionary<string, object?> {
                    ["contentType"] = "application/vnd.microsoft.card.adaptive",
                    ["contentUrl"] = null,
                    ["content"] = RenderAdaptiveCard(request.AdaptiveCard ?? CreateSimpleCard(request))
                }
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    private static TeamsAdaptiveCard CreateSimpleCard(TeamsMessageRequest request) {
        var card = new TeamsAdaptiveCard { FallbackText = request.Text ?? request.Title };
        if (!string.IsNullOrWhiteSpace(request.Title)) {
            card.Body.Add(new TeamsAdaptiveTextBlock { Text = request.Title!, Wrap = true });
        }
        if (!string.IsNullOrWhiteSpace(request.Text)) {
            card.Body.Add(new TeamsAdaptiveTextBlock { Text = request.Text!, Wrap = true });
        }
        if (card.Body.Count == 0) {
            throw new ArgumentException("A Teams message requires an Adaptive Card, title, or text.", nameof(request));
        }
        return card;
    }

    internal static Dictionary<string, object?> RenderAdaptiveCard(TeamsAdaptiveCard card) {
        TeamsAdaptiveCardValidation.Validate(card);
        Dictionary<string, object?>? msTeams = null;
        if (card.AllowImageExpand is not null || card.FullWidth || card.Mentions.Count > 0) {
            msTeams = new Dictionary<string, object?>();
            if (card.AllowImageExpand is not null) {
                msTeams["allowExpand"] = card.AllowImageExpand;
            }

            if (card.FullWidth) {
                msTeams["width"] = "Full";
            }

            if (card.Mentions.Count > 0) {
                msTeams["entities"] = card.Mentions.Select(RenderAdaptiveMention).ToArray();
            }
        }

        var content = new Dictionary<string, object?> {
            ["$schema"] = card.Schema,
            ["type"] = card.Type,
            ["version"] = card.Version,
            ["fallbackText"] = EmptyToNull(card.FallbackText),
            ["minHeight"] = EmptyToNull(card.MinimumHeight),
            ["speak"] = EmptyToNull(card.Speak),
            ["lang"] = EmptyToNull(card.Language),
            ["verticalContentAlignment"] = EmptyToNull(card.VerticalContentAlignment),
            ["backgroundImage"] = RenderBackgroundImage(card.BackgroundImage),
            ["selectAction"] = card.SelectAction is null ? null : RenderAdaptiveAction(card.SelectAction),
            ["body"] = card.Body.Select(RenderAdaptiveElement).ToArray(),
            ["actions"] = card.Actions.Count == 0 ? null : card.Actions.Select(RenderAdaptiveAction).ToArray(),
            ["msteams"] = msTeams
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
                ["id"] = EmptyToNull(textBlock.Id),
                ["spacing"] = EmptyToNull(textBlock.Spacing),
                ["horizontalAlignment"] = EmptyToNull(textBlock.HorizontalAlignment),
                ["wrap"] = textBlock.Wrap,
                ["size"] = EmptyToNull(textBlock.Size),
                ["weight"] = EmptyToNull(textBlock.Weight),
                ["color"] = EmptyToNull(textBlock.Color),
                ["height"] = EmptyToNull(textBlock.Height),
                ["fontType"] = EmptyToNull(textBlock.FontType),
                ["isSubtle"] = textBlock.Subtle,
                ["maxLines"] = textBlock.MaximumLines,
                ["highlight"] = textBlock.Highlight,
                ["italic"] = textBlock.Italic,
                ["strikeThrough"] = textBlock.StrikeThrough,
                ["separator"] = textBlock.Separator,
                ["isVisible"] = textBlock.IsVisible
            };
        }

        if (element is TeamsAdaptiveRichTextBlock richTextBlock) {
            return new Dictionary<string, object?> {
                ["type"] = richTextBlock.Type,
                ["id"] = EmptyToNull(richTextBlock.Id),
                ["horizontalAlignment"] = EmptyToNull(richTextBlock.HorizontalAlignment),
                ["height"] = EmptyToNull(richTextBlock.Height),
                ["spacing"] = EmptyToNull(richTextBlock.Spacing),
                ["separator"] = richTextBlock.Separator,
                ["isVisible"] = richTextBlock.IsVisible,
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
                ["height"] = EmptyToNull(factSet.Height),
                ["spacing"] = EmptyToNull(factSet.Spacing),
                ["separator"] = factSet.Separator,
                ["facts"] = factSet.Facts.Select(fact => new Dictionary<string, object?> {
                    ["title"] = fact.Title,
                    ["value"] = fact.Value
                }).ToArray()
            };
        }

        if (element is TeamsAdaptiveImage image) {
            return new Dictionary<string, object?> {
                ["type"] = image.Type,
                ["id"] = EmptyToNull(image.Id),
                ["url"] = image.Url,
                ["altText"] = EmptyToNull(image.AltText),
                ["size"] = EmptyToNull(image.Size),
                ["style"] = EmptyToNull(image.Style),
                ["horizontalAlignment"] = EmptyToNull(image.HorizontalAlignment),
                ["height"] = EmptyToNull(image.Height),
                ["width"] = EmptyToNull(image.Width),
                ["spacing"] = EmptyToNull(image.Spacing),
                ["backgroundColor"] = EmptyToNull(image.BackgroundColor),
                ["separator"] = image.Separator,
                ["isVisible"] = image.IsVisible,
                ["selectAction"] = image.SelectAction is null ? null : RenderAdaptiveAction(image.SelectAction)
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
                ["id"] = EmptyToNull(imageSet.Id),
                ["imageSize"] = EmptyToNull(imageSet.ImageSize),
                ["horizontalAlignment"] = EmptyToNull(imageSet.HorizontalAlignment),
                ["height"] = EmptyToNull(imageSet.Height),
                ["spacing"] = EmptyToNull(imageSet.Spacing),
                ["separator"] = imageSet.Separator,
                ["isVisible"] = imageSet.IsVisible,
                ["images"] = imageSet.Images.Select(image => (object?)RenderAdaptiveElement(image)).ToArray()
            };
        }

        if (element is TeamsAdaptiveContainer container) {
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
                ["backgroundImage"] = RenderBackgroundImage(container.BackgroundImage),
                ["selectAction"] = container.SelectAction is null ? null : RenderAdaptiveAction(container.SelectAction),
                ["items"] = container.Items.Select(RenderAdaptiveElement).ToArray()
            };
        }

        if (element is TeamsAdaptiveColumn column) {
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
                ["selectAction"] = column.SelectAction is null ? null : RenderAdaptiveAction(column.SelectAction),
                ["items"] = column.Items.Select(RenderAdaptiveElement).ToArray()
            };
        }

        if (element is TeamsAdaptiveColumnSet columnSet) {
            return new Dictionary<string, object?> {
                ["type"] = columnSet.Type,
                ["style"] = EmptyToNull(columnSet.Style),
                ["minHeight"] = EmptyToNull(columnSet.MinimumHeight),
                ["bleed"] = columnSet.Bleed,
                ["horizontalAlignment"] = EmptyToNull(columnSet.HorizontalAlignment),
                ["height"] = EmptyToNull(columnSet.Height),
                ["spacing"] = EmptyToNull(columnSet.Spacing),
                ["separator"] = columnSet.Separator,
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

    private static Dictionary<string, object?> RenderAdaptiveAction(TeamsAdaptiveOpenUrlAction action) => new() {
        ["type"] = action.Type,
        ["id"] = EmptyToNull(action.Id),
        ["title"] = EmptyToNull(action.Title),
        ["url"] = action.Url
    };

    private static Dictionary<string, object?>? RenderBackgroundImage(TeamsAdaptiveBackgroundImage? image) =>
        image is null ? null : new Dictionary<string, object?> {
            ["url"] = EmptyToNull(image.Url),
            ["fillMode"] = EmptyToNull(image.FillMode),
            ["horizontalAlignment"] = EmptyToNull(image.HorizontalAlignment),
            ["verticalAlignment"] = EmptyToNull(image.VerticalAlignment)
        };

    private static string? EmptyToNull(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
