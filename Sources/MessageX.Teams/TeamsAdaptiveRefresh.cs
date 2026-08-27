namespace MessageX.Teams;

/// <summary>Adaptive Card automatic refresh configuration.</summary>
public sealed class TeamsAdaptiveRefresh {
    /// <summary>Action invoked when Teams refreshes the card.</summary>
    public TeamsAdaptiveExecuteAction Action { get; set; } = new();

    /// <summary>Optional Teams user identifiers that receive automatic refreshes.</summary>
    public IList<string> UserIds { get; } = new List<string>();
}
