namespace MessageX.Teams;

public sealed class TeamsAdaptiveSubmitAction : TeamsAdaptiveAction {
    public override string Type => "Action.Submit";

    /// <summary>Capability-free application data included with the submit activity.</summary>
    public MessageDataValue? Data { get; set; }
}
