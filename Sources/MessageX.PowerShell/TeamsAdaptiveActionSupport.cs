using MessageX.Teams;

namespace MessageX.PowerShell;

internal static class TeamsAdaptiveActionSupport {
    internal static TeamsAdaptiveAction? CreateSelectAction(
        string? actionType,
        string? actionId,
        string? actionUrl,
        string? actionTitle,
        IEnumerable<string>? targetElements) {
        if (!string.IsNullOrWhiteSpace(actionUrl) || string.Equals(actionType, "Action.OpenUrl", StringComparison.OrdinalIgnoreCase)) {
            return new TeamsAdaptiveOpenUrlAction {
                Id = actionId,
                Title = actionTitle ?? string.Empty,
                Url = actionUrl ?? string.Empty
            };
        }

        if ((targetElements?.Any(static target => !string.IsNullOrWhiteSpace(target)) ?? false) ||
            string.Equals(actionType, "Action.ToggleVisibility", StringComparison.OrdinalIgnoreCase)) {
            var action = new TeamsAdaptiveToggleVisibilityAction {
                Id = actionId,
                Title = actionTitle ?? string.Empty
            };

            foreach (var targetElement in targetElements ?? Array.Empty<string>()) {
                if (!string.IsNullOrWhiteSpace(targetElement)) {
                    action.TargetElements.Add(targetElement);
                }
            }

            return action;
        }

        return string.Equals(actionType, "Action.Submit", StringComparison.OrdinalIgnoreCase)
            ? new TeamsAdaptiveSubmitAction {
                Id = actionId,
                Title = actionTitle ?? string.Empty
            }
            : null;
    }
}
