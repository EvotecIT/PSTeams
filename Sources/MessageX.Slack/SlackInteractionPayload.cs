using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Safe typed projection of handler-useful Slack interaction data.</summary>
public sealed class SlackInteractionPayload {
    /// <summary>Creates a safe Slack interaction projection.</summary>
    public SlackInteractionPayload(
        SlackActionInput[]? actions,
        SlackViewSubmissionInput? view,
        SlackMessageInput? message)
        : this(actions, view, message, null) {
    }

    /// <summary>Creates a safe Slack interaction projection including state accompanying a block action.</summary>
    [JsonConstructor]
    public SlackInteractionPayload(
        SlackActionInput[]? actions,
        SlackViewSubmissionInput? view,
        SlackMessageInput? message,
        SlackViewStateInput[]? state) {
        Actions = actions ?? Array.Empty<SlackActionInput>();
        View = view;
        Message = message;
        State = state ?? Array.Empty<SlackViewStateInput>();
    }

    /// <summary>Action or selection values supplied by the interaction.</summary>
    public SlackActionInput[] Actions { get; }

    /// <summary>Modal view state when the interaction is a view submission.</summary>
    public SlackViewSubmissionInput? View { get; }

    /// <summary>Selected-message data for message shortcuts.</summary>
    public SlackMessageInput? Message { get; }

    /// <summary>Current bounded input state accompanying a block action.</summary>
    public SlackViewStateInput[] State { get; }
}

/// <summary>Safe normalized value from one Slack block action or selection.</summary>
public sealed class SlackActionInput {
    /// <summary>Creates one normalized Slack action value.</summary>
    public SlackActionInput(
        string actionId,
        string type,
        string? blockId,
        string? value,
        string[]? selectedValues)
        : this(actionId, type, blockId, value, selectedValues, null) {
    }

    /// <summary>Creates one normalized Slack action value, including rich-text input.</summary>
    [JsonConstructor]
    public SlackActionInput(
        string actionId,
        string type,
        string? blockId,
        string? value,
        string[]? selectedValues,
        JsonElement? richTextValue) {
        ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        BlockId = blockId;
        Value = value;
        SelectedValues = selectedValues ?? Array.Empty<string>();
        RichTextValue = CloneRichText(richTextValue);
    }

    /// <summary>Provider action identifier.</summary>
    public string ActionId { get; }

    /// <summary>Provider action element type.</summary>
    public string Type { get; }

    /// <summary>Owning block identifier when supplied.</summary>
    public string? BlockId { get; }

    /// <summary>Scalar submitted value when supplied.</summary>
    public string? Value { get; }

    /// <summary>Normalized selected values for selection inputs.</summary>
    public string[] SelectedValues { get; }

    /// <summary>Bounded capability-free rich-text input when supplied.</summary>
    public JsonElement? RichTextValue { get; }

    private static JsonElement? CloneRichText(JsonElement? value) {
        if (value is null) {
            return null;
        }
        if (value.Value.ValueKind != JsonValueKind.Object) {
            throw new ArgumentException("A Slack rich-text input must be a JSON object.", nameof(value));
        }
        return value.Value.Clone();
    }
}

/// <summary>Safe normalized modal submission.</summary>
public sealed class SlackViewSubmissionInput {
    /// <summary>Creates a normalized Slack view submission.</summary>
    public SlackViewSubmissionInput(string callbackId, SlackViewStateInput[]? values) {
        CallbackId = callbackId ?? throw new ArgumentNullException(nameof(callbackId));
        Values = values ?? Array.Empty<SlackViewStateInput>();
    }

    /// <summary>Creates a normalized Slack view submission including safe modal metadata.</summary>
    [JsonConstructor]
    public SlackViewSubmissionInput(
        string callbackId,
        SlackViewStateInput[]? values,
        string? privateMetadata) {
        CallbackId = callbackId ?? throw new ArgumentNullException(nameof(callbackId));
        Values = values ?? Array.Empty<SlackViewStateInput>();
        PrivateMetadata = privateMetadata;
    }

    /// <summary>Provider view callback identifier.</summary>
    public string CallbackId { get; }

    /// <summary>Submitted modal state values.</summary>
    public SlackViewStateInput[] Values { get; }

    /// <summary>Opaque application-owned modal metadata supplied by Slack.</summary>
    public string? PrivateMetadata { get; }
}

/// <summary>Safe normalized value from a Slack modal state entry.</summary>
public sealed class SlackViewStateInput {
    /// <summary>Creates one normalized modal state value.</summary>
    public SlackViewStateInput(
        string blockId,
        string actionId,
        string type,
        string? value,
        string[]? selectedValues)
        : this(blockId, actionId, type, value, selectedValues, null, null) {
    }

    /// <summary>Creates one normalized modal state value, including file-input identifiers.</summary>
    public SlackViewStateInput(
        string blockId,
        string actionId,
        string type,
        string? value,
        string[]? selectedValues,
        string[]? fileIds)
        : this(blockId, actionId, type, value, selectedValues, fileIds, null) {
    }

    /// <summary>Creates one normalized modal state value, including rich-text and file inputs.</summary>
    [JsonConstructor]
    public SlackViewStateInput(
        string blockId,
        string actionId,
        string type,
        string? value,
        string[]? selectedValues,
        string[]? fileIds,
        JsonElement? richTextValue) {
        BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
        ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Value = value;
        SelectedValues = selectedValues ?? Array.Empty<string>();
        FileIds = fileIds ?? Array.Empty<string>();
        RichTextValue = CloneRichText(richTextValue);
    }

    /// <summary>Owning block identifier.</summary>
    public string BlockId { get; }

    /// <summary>Provider action identifier.</summary>
    public string ActionId { get; }

    /// <summary>Provider input element type.</summary>
    public string Type { get; }

    /// <summary>Scalar submitted value when supplied.</summary>
    public string? Value { get; }

    /// <summary>Normalized selected values for selection inputs.</summary>
    public string[] SelectedValues { get; }

    /// <summary>Slack file identifiers submitted by a file input.</summary>
    public string[] FileIds { get; }

    /// <summary>Bounded capability-free rich-text input when supplied.</summary>
    public JsonElement? RichTextValue { get; }

    private static JsonElement? CloneRichText(JsonElement? value) {
        if (value is null) {
            return null;
        }
        if (value.Value.ValueKind != JsonValueKind.Object) {
            throw new ArgumentException("A Slack rich-text input must be a JSON object.", nameof(value));
        }
        return value.Value.Clone();
    }
}

/// <summary>Safe selected-message data for a Slack message shortcut.</summary>
public sealed class SlackMessageInput {
    /// <summary>Creates selected-message data.</summary>
    [JsonConstructor]
    public SlackMessageInput(string timestamp, string? text) {
        Timestamp = timestamp ?? throw new ArgumentNullException(nameof(timestamp));
        Text = text;
    }

    /// <summary>Selected Slack message timestamp.</summary>
    public string Timestamp { get; }

    /// <summary>Selected message text when supplied.</summary>
    public string? Text { get; }
}
