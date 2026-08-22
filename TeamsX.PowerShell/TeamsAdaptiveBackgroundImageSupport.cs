using TeamsX;

namespace TeamsX.PowerShell;

internal static class TeamsAdaptiveBackgroundImageSupport {
    internal static TeamsAdaptiveBackgroundImage? Create(
        string? url,
        string? fillMode,
        string? horizontalAlignment,
        string? verticalAlignment) {
        if (string.IsNullOrWhiteSpace(url) &&
            string.IsNullOrWhiteSpace(fillMode) &&
            string.IsNullOrWhiteSpace(horizontalAlignment) &&
            string.IsNullOrWhiteSpace(verticalAlignment)) {
            return null;
        }

        return new TeamsAdaptiveBackgroundImage {
            Url = url,
            FillMode = fillMode,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment
        };
    }
}
