namespace MessageX.Teams;

/// <summary>A Universal Action handled by an installed Teams app.</summary>
public sealed class TeamsAdaptiveExecuteAction : TeamsAdaptiveAction {
    /// <inheritdoc />
    public override string Type => "Action.Execute";

    /// <summary>Application-defined verb used to route the invoke activity.</summary>
    public string Verb { get; set; } = string.Empty;

    /// <summary>Capability-free application data included with the invoke activity.</summary>
    public MessageDataValue? Data { get; set; }

    /// <summary>Controls whether card inputs accompany the action.</summary>
    public TeamsAdaptiveAssociatedInputs AssociatedInputs { get; set; } = TeamsAdaptiveAssociatedInputs.Auto;

    /// <summary>Optional legacy fallback for clients that cannot render <c>Action.Execute</c>.</summary>
    public TeamsAdaptiveSubmitAction? Fallback { get; set; }
}
