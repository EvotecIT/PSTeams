namespace MessageX.Teams;

internal static class TeamsAdaptiveCardValidation {
    private const int MaximumNestingDepth = 32;

    public static void Validate(TeamsAdaptiveCard card, bool allowUniversalActions = true) {
        if (card is null) {
            throw new ArgumentNullException(nameof(card));
        }

        ValidateCard(card, new HashSet<TeamsAdaptiveCard>(), 0, allowUniversalActions);
    }

    private static void ValidateCard(
        TeamsAdaptiveCard card,
        HashSet<TeamsAdaptiveCard> visitedCards,
        int depth,
        bool allowUniversalActions) {
        if (depth > MaximumNestingDepth) {
            throw new ArgumentException("Adaptive Card nesting exceeds the supported depth.", nameof(card));
        }
        if (!visitedCards.Add(card)) {
            throw new ArgumentException("Adaptive Cards cannot contain recursive ShowCard references.", nameof(card));
        }

        var actions = EnumerateActions(card).ToArray();
        var usesUniversalActions = card.Refresh is not null || actions.Any(static action => action is TeamsAdaptiveExecuteAction);
        if (usesUniversalActions &&
            (!Version.TryParse(card.Version, out var version) || version < new Version(1, 5))) {
            throw new ArgumentException(
                "Teams Universal Actions and refresh require Adaptive Card version 1.5 or later.",
                nameof(card));
        }
        if (usesUniversalActions && !allowUniversalActions) {
            throw new ArgumentException(
                "Teams webhook delivery does not support Action.Execute or refresh without a bot-capable outbound transport.",
                nameof(card));
        }

        if (card.Refresh is not null) {
            ValidateExecute(card.Refresh.Action, nameof(card.Refresh));
            ValidateCoordinates(card.Refresh.UserIds, "refresh user identifiers", 60);
        }
        foreach (var action in actions) {
            if (action is TeamsAdaptiveExecuteAction execute) {
                ValidateExecute(execute, nameof(card.Actions));
            }
            if (action is TeamsAdaptiveShowCardAction { Card: not null } showCard) {
                ValidateCard(showCard.Card, visitedCards, depth + 1, allowUniversalActions);
            }
        }

        visitedCards.Remove(card);
    }

    private static IEnumerable<TeamsAdaptiveAction> EnumerateActions(TeamsAdaptiveCard card) {
        if (card.SelectAction is not null) {
            yield return card.SelectAction;
        }
        foreach (var action in card.Actions) {
            yield return action;
        }
        foreach (var element in card.Body) {
            foreach (var action in EnumerateActions(element)) {
                yield return action;
            }
        }
    }

    private static IEnumerable<TeamsAdaptiveAction> EnumerateActions(TeamsAdaptiveCardElement element) {
        switch (element) {
            case TeamsAdaptiveImage image when image.SelectAction is not null:
                yield return image.SelectAction;
                break;
            case TeamsAdaptiveImageSet imageSet:
                foreach (var image in imageSet.Images) {
                    foreach (var action in EnumerateActions(image)) {
                        yield return action;
                    }
                }
                break;
            case TeamsAdaptiveActionSet actionSet:
                foreach (var action in actionSet.Actions) {
                    yield return action;
                }
                break;
            case TeamsAdaptiveContainer container:
                if (container.SelectAction is not null) {
                    yield return container.SelectAction;
                }
                foreach (var child in container.Items) {
                    foreach (var action in EnumerateActions(child)) {
                        yield return action;
                    }
                }
                break;
            case TeamsAdaptiveColumnSet columnSet:
                foreach (var column in columnSet.Columns) {
                    if (column.SelectAction is not null) {
                        yield return column.SelectAction;
                    }
                    foreach (var child in column.Items) {
                        foreach (var action in EnumerateActions(child)) {
                            yield return action;
                        }
                    }
                }
                break;
        }
    }

    private static void ValidateExecute(TeamsAdaptiveExecuteAction action, string parameterName) {
        if (string.IsNullOrWhiteSpace(action.Verb) || action.Verb.Length > 64 || action.Verb.Any(char.IsControl)) {
            throw new ArgumentException(
                "Teams Action.Execute verbs must contain 1 to 64 non-control characters.",
                parameterName);
        }
        if (action.AssociatedInputs is not TeamsAdaptiveAssociatedInputs.Auto and not TeamsAdaptiveAssociatedInputs.None) {
            throw new ArgumentException("Teams Action.Execute associated-input policies must be auto or none.", parameterName);
        }
        if (action.Data is not null && action.Data.Kind != MessageDataValueKind.Object) {
            throw new ArgumentException("Teams Action.Execute data must be a JSON object.", parameterName);
        }
        if (action.Fallback?.Data is not null && action.Fallback.Data.Kind != MessageDataValueKind.Object) {
            throw new ArgumentException("Teams Action.Submit fallback data must be a JSON object.", parameterName);
        }
    }

    private static void ValidateCoordinates(IEnumerable<string> values, string label, int maximumCount) {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values) {
            if (seen.Count >= maximumCount) {
                throw new ArgumentException($"Teams {label} cannot contain more than {maximumCount} values.", label);
            }
            if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || value.Any(char.IsControl) || !seen.Add(value)) {
                throw new ArgumentException($"Teams {label} must be unique bounded values.", label);
            }
        }
    }
}
