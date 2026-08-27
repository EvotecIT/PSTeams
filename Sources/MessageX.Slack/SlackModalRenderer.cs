using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Slack;

internal static class SlackModalRenderer {
    private static readonly JsonSerializerOptions Options = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string RenderOpen(string triggerId, SlackModalView view) {
        if (string.IsNullOrWhiteSpace(triggerId) || triggerId.Length > 255 || triggerId.Any(char.IsControl)) {
            throw new InvalidOperationException("The verified Slack interaction does not contain a usable modal trigger.");
        }
        SlackMessageValidator.ValidateModal(view);
        var payload = new Dictionary<string, object?> {
            ["trigger_id"] = triggerId,
            ["view"] = new Dictionary<string, object?> {
                ["type"] = "modal",
                ["callback_id"] = view.CallbackId,
                ["title"] = SlackMessageRenderer.RenderText(view.Title),
                ["submit"] = view.Submit is null ? null : SlackMessageRenderer.RenderText(view.Submit),
                ["close"] = view.Close is null ? null : SlackMessageRenderer.RenderText(view.Close),
                ["notify_on_close"] = view.NotifyOnClose,
                ["blocks"] = view.Blocks.Select(SlackMessageRenderer.RenderBlock).ToArray()
            }
        };
        return JsonSerializer.Serialize(payload, Options);
    }
}
