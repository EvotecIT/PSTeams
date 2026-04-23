using System.Net;

namespace TeamsX;

internal static class GraphMessageRenderer {
    public static string Render(TeamsMessageRequest request, TeamsDeliveryMethod deliveryMethod) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.AdaptiveCard is not null) {
            return RenderAdaptiveCardMessage(request, deliveryMethod);
        }

        return RenderHtmlMessage(request, deliveryMethod);
    }

    private static string RenderAdaptiveCardMessage(TeamsMessageRequest request, TeamsDeliveryMethod deliveryMethod) {
        ValidateAdaptiveCardForGraph(request.AdaptiveCard!);

        var attachmentId = Guid.NewGuid().ToString("D");
        var bodyFragments = BuildBodyFragments(request);
        bodyFragments.Add($"<attachment id=\"{attachmentId}\"></attachment>");

        var payload = new Dictionary<string, object?> {
            ["subject"] = deliveryMethod is TeamsDeliveryMethod.GraphChannelMessage
                ? EmptyToNull(request.Title)
                : null,
            ["body"] = new Dictionary<string, object?> {
                ["contentType"] = "html",
                ["content"] = string.Join(string.Empty, bodyFragments)
            },
            ["attachments"] = new[] {
                new Dictionary<string, object?> {
                    ["id"] = attachmentId,
                    ["contentType"] = "application/vnd.microsoft.card.adaptive",
                    ["content"] = TeamsJsonSerializer.Serialize(WebhookMessageRenderer.RenderAdaptiveCard(request.AdaptiveCard!)),
                    ["name"] = EmptyToNull(request.EffectiveSummary)
                }
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    private static string RenderHtmlMessage(TeamsMessageRequest request, TeamsDeliveryMethod deliveryMethod) {
        var bodyFragments = BuildBodyFragments(request);
        if (bodyFragments.Count == 0) {
            bodyFragments.Add(WrapParagraph(request.EffectiveSummary));
        }

        var payload = new Dictionary<string, object?> {
            ["subject"] = deliveryMethod is TeamsDeliveryMethod.GraphChannelMessage
                ? EmptyToNull(request.Title)
                : null,
            ["body"] = new Dictionary<string, object?> {
                ["contentType"] = "html",
                ["content"] = string.Join(string.Empty, bodyFragments)
            }
        };

        return TeamsJsonSerializer.Serialize(payload);
    }

    private static List<string> BuildBodyFragments(TeamsMessageRequest request) {
        var fragments = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Title)) {
            fragments.Add($"<div><strong>{Encode(request.Title)}</strong></div>");
        }

        if (!string.IsNullOrWhiteSpace(request.Text)) {
            fragments.Add(WrapParagraph(request.Text));
        }

        foreach (var section in request.Sections) {
            var sectionFragment = RenderSection(section);
            if (!string.IsNullOrWhiteSpace(sectionFragment)) {
                fragments.Add(sectionFragment);
            }
        }

        if (fragments.Count == 0 && !string.IsNullOrWhiteSpace(request.Summary)) {
            fragments.Add(WrapParagraph(request.Summary));
        }

        return fragments;
    }

    private static string RenderSection(TeamsMessageSection section) {
        var fragments = new List<string>();

        if (!string.IsNullOrWhiteSpace(section.Title)) {
            fragments.Add($"<div><strong>{Encode(section.Title)}</strong></div>");
        }

        if (!string.IsNullOrWhiteSpace(section.ActivityTitle)) {
            fragments.Add($"<div><strong>{Encode(section.ActivityTitle)}</strong></div>");
        }

        if (!string.IsNullOrWhiteSpace(section.ActivitySubtitle)) {
            fragments.Add($"<div>{Encode(section.ActivitySubtitle)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(section.ActivityText)) {
            fragments.Add(WrapParagraph(section.ActivityText));
        }

        if (!string.IsNullOrWhiteSpace(section.Text)) {
            fragments.Add(WrapParagraph(section.Text));
        }

        if (section.Facts.Count > 0) {
            var facts = string.Join(string.Empty, section.Facts.Select(fact =>
                $"<li><strong>{Encode(fact.Name)}</strong>: {Encode(fact.Value)}</li>"));
            fragments.Add($"<ul>{facts}</ul>");
        }

        if (section.Buttons.Count > 0) {
            var buttons = string.Join(" ", section.Buttons
                .Where(button => !string.IsNullOrWhiteSpace(button.Link))
                .Select(button => $"<a href=\"{EncodeAttribute(button.Link)}\">{Encode(button.Name)}</a>"));
            if (!string.IsNullOrWhiteSpace(buttons)) {
                fragments.Add($"<div>{buttons}</div>");
            }
        }

        return string.Join(string.Empty, fragments);
    }

    private static string WrapParagraph(string? value) {
        return $"<p>{EncodeMultiline(value)}</p>";
    }

    private static string EncodeMultiline(string? value) {
        return Encode(value).Replace("\r\n", "<br/>")
            .Replace("\n", "<br/>");
    }

    private static string Encode(string? value) {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }

    private static string EncodeAttribute(string? value) {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }

    private static string? EmptyToNull(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void ValidateAdaptiveCardForGraph(TeamsAdaptiveCard card) {
        foreach (var action in card.Actions) {
            ValidateAction(action);
        }

        if (card.SelectAction is not null) {
            ValidateAction(card.SelectAction);
        }

        foreach (var element in card.Body) {
            ValidateElement(element);
        }
    }

    private static void ValidateElement(TeamsAdaptiveCardElement element) {
        switch (element) {
            case TeamsAdaptiveContainer container:
                if (container.SelectAction is not null) {
                    ValidateAction(container.SelectAction);
                }
                foreach (var item in container.Items) {
                    ValidateElement(item);
                }
                return;
            case TeamsAdaptiveColumn column:
                if (column.SelectAction is not null) {
                    ValidateAction(column.SelectAction);
                }
                foreach (var item in column.Items) {
                    ValidateElement(item);
                }
                return;
            case TeamsAdaptiveColumnSet columnSet:
                foreach (var columnItem in columnSet.Columns) {
                    ValidateElement(columnItem);
                }
                return;
            case TeamsAdaptiveImage image:
                if (image.SelectAction is not null) {
                    ValidateAction(image.SelectAction);
                }
                return;
            case TeamsAdaptiveActionSet actionSet:
                foreach (var action in actionSet.Actions) {
                    ValidateAction(action);
                }
                return;
            case TeamsAdaptiveImageSet:
            case TeamsAdaptiveMedia:
            case TeamsAdaptiveFactSet:
            case TeamsAdaptiveTextBlock:
            case TeamsAdaptiveRichTextBlock:
                return;
            default:
                return;
        }
    }

    private static void ValidateAction(TeamsAdaptiveAction action) {
        if (action is TeamsAdaptiveOpenUrlAction) {
            return;
        }

        throw new NotSupportedException($"Adaptive action '{action.Type}' is not supported for Graph chat messages. Only Action.OpenUrl is supported.");
    }
}
