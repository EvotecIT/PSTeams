using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Slack;

internal static class SlackMessageRenderer {
    private static readonly JsonSerializerOptions Options = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Render(SlackMessageRequest message, SlackMessageTarget target) {
        SlackMessageValidator.Validate(message);
        ValidateTarget(target);

        var payload = CreateMessagePayload(message);
        if (target.DeliveryMethod == SlackDeliveryMethod.WebApi) {
            payload["channel"] = target.ConversationId;
        }
        if (!string.IsNullOrWhiteSpace(message.ThreadTimestamp)) {
            payload["thread_ts"] = message.ThreadTimestamp;
            if (message.ReplyBroadcast) {
                payload["reply_broadcast"] = true;
            }
        }
        if (message.UnfurlLinks is not null) {
            payload["unfurl_links"] = message.UnfurlLinks;
        }
        if (message.UnfurlMedia is not null) {
            payload["unfurl_media"] = message.UnfurlMedia;
        }

        return JsonSerializer.Serialize(payload, Options);
    }

    public static string RenderUpdate(SlackMessageRequest message, string conversationId, string timestamp) {
        SlackMessageValidator.Validate(message);
        SlackMessageTarget.ValidateConversationId(conversationId);
        if (SlackMessageValidator.ParseTimestamp(timestamp) is null) {
            throw new ArgumentException(
                "Slack message updates require a valid message timestamp.",
                nameof(timestamp));
        }
        if (!string.IsNullOrWhiteSpace(message.ThreadTimestamp) || message.ReplyBroadcast) {
            throw new ArgumentException(
                "Slack message updates use the persisted message reference and cannot change thread placement.",
                nameof(message));
        }
        if (message.UnfurlLinks is not null || message.UnfurlMedia is not null) {
            throw new ArgumentException(
                "Slack message updates do not accept send-only unfurl options.",
                nameof(message));
        }

        var payload = CreateMessagePayload(message);
        // Slack retains omitted fields during chat.update. Emit both mutable
        // fields so the request represents a replacement rather than a merge.
        if (!payload.ContainsKey("text")) {
            payload["text"] = string.Empty;
        }
        if (!payload.ContainsKey("blocks")) {
            payload["blocks"] = Array.Empty<object>();
        }
        payload["channel"] = conversationId;
        payload["ts"] = timestamp;
        return JsonSerializer.Serialize(payload, Options);
    }

    private static Dictionary<string, object?> CreateMessagePayload(SlackMessageRequest message) {
        var payload = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(message.Text)) {
            payload["text"] = message.Text;
        }
        if (message.Blocks.Count > 0) {
            payload["blocks"] = message.Blocks.Select(RenderBlock).ToArray();
        }
        return payload;
    }

    private static void ValidateTarget(SlackMessageTarget target) {
        if (target is null) {
            throw new ArgumentNullException(nameof(target));
        }
        if (target.DeliveryMethod == SlackDeliveryMethod.IncomingWebhook) {
            SlackMessageTarget.ValidateWebhookUri(target.WebhookUri);
        }
        else if (target.DeliveryMethod == SlackDeliveryMethod.WebApi) {
            SlackMessageTarget.ValidateConversationId(target.ConversationId);
        } else {
            throw new ArgumentException("Unsupported Slack delivery method.", nameof(target));
        }
    }

    private static Dictionary<string, object?> RenderBlock(SlackBlock block) {
        if (block is SlackSectionBlock section) {
            var payload = new Dictionary<string, object?> { ["type"] = section.Type };
            AddOptional(payload, "block_id", section.BlockId);
            AddOptional(payload, "text", section.Text is null ? null : RenderText(section.Text));
            AddOptional(payload, "fields", section.Fields.Count == 0 ? null : section.Fields.Select(RenderText).ToArray());
            AddOptional(payload, "expand", section.Expand);
            return payload;
        }
        if (block is SlackDividerBlock divider) {
            var payload = new Dictionary<string, object?> { ["type"] = divider.Type };
            AddOptional(payload, "block_id", divider.BlockId);
            return payload;
        }

        throw new ArgumentException($"Unsupported Slack block type '{block.GetType().Name}'.", nameof(block));
    }

    private static Dictionary<string, object?> RenderText(SlackTextObject text) {
        var payload = new Dictionary<string, object?> {
            ["type"] = text.Style == SlackTextStyle.Markdown ? "mrkdwn" : "plain_text",
            ["text"] = text.Text
        };
        if (text.Style == SlackTextStyle.PlainText) {
            AddOptional(payload, "emoji", text.Emoji);
        } else {
            AddOptional(payload, "verbatim", text.Verbatim);
        }
        return payload;
    }

    private static void AddOptional(Dictionary<string, object?> payload, string name, object? value) {
        if (value is not null) {
            payload[name] = value;
        }
    }
}
