using MessageX.Teams;

namespace MessageX.PowerShell;

internal static class TeamsAdaptiveActionSupport {
    internal static TeamsAdaptiveOpenUrlAction? CreateSelectAction(
        string? actionId, string? actionUrl, string? actionTitle) =>
        string.IsNullOrWhiteSpace(actionUrl) ? null : new TeamsAdaptiveOpenUrlAction {
            Id = actionId,
            Title = actionTitle ?? string.Empty,
            Url = actionUrl!
        };
}
