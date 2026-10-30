namespace MessageX.Teams;

internal static class TeamsAdaptiveCardValidation {
    private const int MaximumNestingDepth = 32;

    public static void Validate(TeamsAdaptiveCard card) {
        if (card is null) throw new ArgumentNullException(nameof(card));
        ValidateAction(card.SelectAction);
        foreach (var action in card.Actions) ValidateAction(action);
        var active = new HashSet<TeamsAdaptiveCardElement>();
        foreach (var element in card.Body) ValidateElement(element, active, 0);
    }

    private static void ValidateElement(TeamsAdaptiveCardElement element,
        HashSet<TeamsAdaptiveCardElement> active, int depth) {
        if (element is null) throw new ArgumentException("Adaptive Card elements cannot be null.");
        if (depth > MaximumNestingDepth || !active.Add(element)) {
            throw new ArgumentException("Adaptive Card elements exceed the nesting limit or contain a cycle.");
        }
        switch (element) {
            case TeamsAdaptiveImage image:
                ValidateAction(image.SelectAction);
                break;
            case TeamsAdaptiveImageSet imageSet:
                foreach (var image in imageSet.Images) ValidateElement(image, active, depth + 1);
                break;
            case TeamsAdaptiveActionSet actionSet:
                foreach (var action in actionSet.Actions) ValidateAction(action);
                break;
            case TeamsAdaptiveContainer container:
                ValidateAction(container.SelectAction);
                foreach (var child in container.Items) ValidateElement(child, active, depth + 1);
                break;
            case TeamsAdaptiveColumnSet columnSet:
                foreach (var column in columnSet.Columns) ValidateElement(column, active, depth + 1);
                break;
            case TeamsAdaptiveColumn column:
                ValidateAction(column.SelectAction);
                foreach (var child in column.Items) ValidateElement(child, active, depth + 1);
                break;
        }
        active.Remove(element);
    }

    private static void ValidateAction(TeamsAdaptiveOpenUrlAction? action) {
        if (action is null) return;
        if (!Uri.TryCreate(action.Url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps) {
            throw new ArgumentException("Teams Workflow cards support only Action.OpenUrl with an absolute HTTPS URL.");
        }
    }
}
