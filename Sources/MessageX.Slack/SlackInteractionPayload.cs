using System.Text.Json.Serialization;

namespace MessageX.Slack;

/// <summary>Safe typed projection of handler-useful Slack interaction data.</summary>
public sealed class SlackInteractionPayload {
    /// <summary>Creates a safe Slack interaction projection.</summary>
    [JsonConstructor]
    public SlackInteractionPayload(
        SlackActionInput[]? actions,
        SlackViewSubmissionInput? view,
        SlackMessageInput? message) {
        Actions = actions ?? Array.Empty<SlackActionInput>();
        View = view;
        Message = message;
    }

    /// <summary>Action or selection values supplied by the interaction.</summary>
    public SlackActionInput[] Actions { get; }

    /// <summary>Modal view state when the interaction is a view submission.</summary>
    public SlackViewSubmissionInput? View { get; }

    /// <summary>Selected-message data for message shortcuts.</summary>
    public SlackMessageInput? Message { get; }
}

/// <summary>Safe normalized value from one Slack block action or selection.</summary>
public sealed class SlackActionInput {
    /// <summary>Creates one normalized Slack action value.</summary>
    [JsonConstructor]
    public SlackActionInput(
        string actionId,
        string type,
        string? blockId,
        string? value,
        string[]? selectedValues) {
        ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        BlockId = blockId;
        Value = value;
        SelectedValues = selectedValues ?? Array.Empty<string>();
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
}

/// <summary>Safe normalized modal submission.</summary>
public sealed class SlackViewSubmissionInput {
    /// <summary>Creates a normalized Slack view submission.</summary>
    [JsonConstructor]
    public SlackViewSubmissionInput(string callbackId, SlackViewStateInput[]? values) {
        CallbackId = callbackId ?? throw new ArgumentNullException(nameof(callbackId));
        Values = values ?? Array.Empty<SlackViewStateInput>();
    }

    /// <summary>Provider view callback identifier.</summary>
    public string CallbackId { get; }

    /// <summary>Submitted modal state values.</summary>
    public SlackViewStateInput[] Values { get; }
}

/// <summary>Safe normalized value from a Slack modal state entry.</summary>
public sealed class SlackViewStateInput {
    /// <summary>Creates one normalized modal state value.</summary>
    [JsonConstructor]
    public SlackViewStateInput(
        string blockId,
        string actionId,
        string type,
        string? value,
        string[]? selectedValues) {
        BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
        ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Value = value;
        SelectedValues = selectedValues ?? Array.Empty<string>();
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
