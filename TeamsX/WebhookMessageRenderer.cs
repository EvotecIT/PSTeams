namespace TeamsX;

public static class WebhookMessageRenderer {
    public static string Render(TeamsMessageRequest request) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.AdaptiveCard is not null) {
            return RenderAdaptiveCardMessage(request);
        }

        if (request.UseConnectorCardFormat) {
            return RenderConnectorCardMessage(request);
        }

        var payload = new Dictionary<string, object?> {
            ["summary"] = EmptyToNull(request.EffectiveSummary),
            ["title"] = EmptyToNull(request.Title),
            ["text"] = EmptyToNull(request.Text)
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    private static string RenderConnectorCardMessage(TeamsMessageRequest request) {
        var payload = new Dictionary<string, object?> {
            ["themeColor"] = EmptyToNull(request.ThemeColor),
            ["title"] = EmptyToNull(request.Title),
            ["hideOriginalBody"] = request.HideOriginalBody ? true : null,
            ["summary"] = EmptyToNull(request.EffectiveSummary),
            ["text"] = EmptyToNull(request.Text),
            ["sections"] = request.Sections.Count == 0
                ? null
                : request.Sections.Select(RenderConnectorSection).ToArray()
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
            ["backgroundImage"] = card.BackgroundImage,
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

    private static Dictionary<string, object?> RenderConnectorSection(TeamsMessageSection section) {
        var text = section.Text;
        if (section.HeroImages.Count > 0) {
            var fragments = new List<string>(section.HeroImages);
            if (!string.IsNullOrWhiteSpace(text)) {
                fragments.Add(text!);
            }

            text = string.Join(" ", fragments);
        }

        return new Dictionary<string, object?> {
            ["title"] = EmptyToNull(section.Title),
            ["activityTitle"] = EmptyToNull(section.ActivityTitle),
            ["activitySubtitle"] = EmptyToNull(section.ActivitySubtitle),
            ["activityImage"] = EmptyToNull(section.ActivityImage),
            ["activityText"] = EmptyToNull(section.ActivityText),
            ["text"] = EmptyToNull(text),
            ["startGroup"] = section.StartGroup ? true : null,
            ["facts"] = section.Facts.Count == 0
                ? null
                : section.Facts.Select(fact => new Dictionary<string, object?> {
                    ["name"] = EmptyToNull(fact.Name),
                    ["value"] = EmptyToNull(fact.Value)
                }).ToArray(),
            ["potentialAction"] = section.Buttons.Count == 0
                ? null
                : section.Buttons.Select(RenderConnectorButton).ToArray(),
            ["images"] = section.Images.Count == 0
                ? null
                : section.Images.Select(image => new Dictionary<string, object?> {
                    ["image"] = EmptyToNull(image)
                }).ToArray()
        };
    }

    private static Dictionary<string, object?> RenderConnectorButton(TeamsMessageButton button) {
        return button.ButtonType switch {
            TeamsMessageButtonType.ViewAction => new Dictionary<string, object?> {
                ["@context"] = "http://schema.org",
                ["@type"] = "ViewAction",
                ["name"] = EmptyToNull(button.Name),
                ["target"] = string.IsNullOrWhiteSpace(button.Link) ? null : new[] { button.Link }
            },
            TeamsMessageButtonType.TextInput => new Dictionary<string, object?> {
                ["@type"] = "ActionCard",
                ["Name"] = EmptyToNull(button.Name),
                ["Inputs"] = new[] {
                    new Dictionary<string, object?> {
                        ["@type"] = "TextInput",
                        ["id"] = "Comment",
                        ["isMultiLine"] = true,
                        ["title"] = "Enter Your Text Input Here"
                    }
                },
                ["actions"] = new[] {
                    new Dictionary<string, object?> {
                        ["@type"] = "HttpPOST",
                        ["Name"] = "OK",
                        ["target"] = EmptyToNull(button.Link)
                    }
                }
            },
            TeamsMessageButtonType.DateInput => new Dictionary<string, object?> {
                ["@type"] = "ActionCard",
                ["Name"] = EmptyToNull(button.Name),
                ["Inputs"] = new[] {
                    new Dictionary<string, object?> {
                        ["@type"] = "DateInput",
                        ["id"] = "dueDate"
                    }
                },
                ["actions"] = new[] {
                    new Dictionary<string, object?> {
                        ["@type"] = "HttpPOST",
                        ["Name"] = "OK",
                        ["target"] = EmptyToNull(button.Link)
                    }
                }
            },
            TeamsMessageButtonType.HttpPost => new Dictionary<string, object?> {
                ["name"] = EmptyToNull(button.Name),
                ["@type"] = "HttpPOST",
                ["Target"] = EmptyToNull(button.Link)
            },
            TeamsMessageButtonType.OpenUri => new Dictionary<string, object?> {
                ["name"] = EmptyToNull(button.Name),
                ["@type"] = "OpenURI",
                ["Targets"] = string.IsNullOrWhiteSpace(button.Link)
                    ? null
                    : new[] {
                        new Dictionary<string, object?> {
                            ["os"] = "default",
                            ["uri"] = button.Link
                        }
                    }
            },
            _ => throw new NotSupportedException($"Connector action '{button.ButtonType}' is not supported by the webhook renderer yet.")
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
                ["backgroundImage"] = container.BackgroundImage,
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

    private static Dictionary<string, object?> RenderAdaptiveAction(TeamsAdaptiveAction action) {
        if (action is TeamsAdaptiveOpenUrlAction openUrlAction) {
            return new Dictionary<string, object?> {
                ["type"] = openUrlAction.Type,
                ["id"] = EmptyToNull(openUrlAction.Id),
                ["title"] = EmptyToNull(openUrlAction.Title),
                ["url"] = openUrlAction.Url
            };
        }

        if (action is TeamsAdaptiveToggleVisibilityAction toggleVisibilityAction) {
            return new Dictionary<string, object?> {
                ["type"] = toggleVisibilityAction.Type,
                ["id"] = EmptyToNull(toggleVisibilityAction.Id),
                ["title"] = EmptyToNull(toggleVisibilityAction.Title),
                ["targetElements"] = toggleVisibilityAction.TargetElements.ToArray()
            };
        }

        if (action is TeamsAdaptiveSubmitAction submitAction) {
            return new Dictionary<string, object?> {
                ["type"] = submitAction.Type,
                ["id"] = EmptyToNull(submitAction.Id),
                ["title"] = EmptyToNull(submitAction.Title)
            };
        }

        if (action is TeamsAdaptiveShowCardAction showCardAction) {
            return new Dictionary<string, object?> {
                ["type"] = showCardAction.Type,
                ["id"] = EmptyToNull(showCardAction.Id),
                ["title"] = EmptyToNull(showCardAction.Title),
                ["card"] = TeamsLegacyAdaptiveNormalizer.Normalize(showCardAction.Card)
            };
        }

        throw new NotSupportedException($"Adaptive action '{action.GetType().Name}' is not supported by the webhook renderer yet.");
    }

    private static string? EmptyToNull(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
