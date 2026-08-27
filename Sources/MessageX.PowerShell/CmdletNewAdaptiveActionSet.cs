using System.Collections;
using System.Linq;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive action set backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveActionSet")]
[OutputType(typeof(TeamsAdaptiveActionSet))]
public sealed class CmdletNewAdaptiveActionSet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Action { get; set; }

    protected override void ProcessRecord() {
        if (Action is null) {
            return;
        }

        var actionSet = new TeamsAdaptiveActionSet();
        foreach (var item in Action.Invoke()) {
            ApplyAction(actionSet, item);
        }

        if (actionSet.Actions.Count > 0) {
            WriteObject(actionSet);
        }
    }

    private static void ApplyAction(TeamsAdaptiveActionSet actionSet, object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            return;
        }

        if (value is TeamsAdaptiveAction adaptiveAction) {
            actionSet.Actions.Add(adaptiveAction);
            return;
        }

        if (value is IDictionary dictionary && TryCreateAction(dictionary, out var converted)) {
            actionSet.Actions.Add(converted);
        }
    }

    private static bool TryCreateAction(IDictionary dictionary, out TeamsAdaptiveAction action) {
        action = null!;
        var type = GetDictionaryString(dictionary, "type");
        var title = GetDictionaryString(dictionary, "title") ?? string.Empty;

        switch (type) {
            case "Action.OpenUrl":
                action = new TeamsAdaptiveOpenUrlAction {
                    Title = title,
                    Url = GetDictionaryString(dictionary, "url") ?? string.Empty
                };
                return true;
            case "Action.Submit":
                action = CreateSubmitAction(dictionary, title);
                return true;
            case "Action.Execute":
                var verb = GetDictionaryString(dictionary, "verb");
                if (string.IsNullOrWhiteSpace(verb)) {
                    return false;
                }
                var execute = new TeamsAdaptiveExecuteAction {
                    Id = GetDictionaryString(dictionary, "id"),
                    Title = title,
                    Verb = verb!,
                    Data = GetDictionaryData(dictionary, "data")
                };
                var associatedInputs = GetDictionaryString(dictionary, "associatedInputs");
                var parsedInputs = TeamsAdaptiveAssociatedInputs.Auto;
                if (associatedInputs is not null &&
                    !Enum.TryParse(associatedInputs, ignoreCase: true, out parsedInputs)) {
                    throw new InvalidOperationException(
                        "Action.Execute associatedInputs must be 'auto' or 'none'.");
                }
                if (associatedInputs is not null) {
                    execute.AssociatedInputs = parsedInputs;
                }
                if (TryGetDictionaryValue(dictionary, "fallback", out var fallbackValue) && fallbackValue is not null) {
                    if (fallbackValue is not IDictionary fallbackDictionary ||
                        !string.Equals(GetDictionaryString(fallbackDictionary, "type"), "Action.Submit", StringComparison.OrdinalIgnoreCase)) {
                        throw new InvalidOperationException(
                            "Action.Execute fallback must be an Action.Submit dictionary.");
                    }
                    execute.Fallback = CreateSubmitAction(
                        fallbackDictionary,
                        GetDictionaryString(fallbackDictionary, "title") ?? title);
                }
                action = execute;
                return true;
            case "Action.ToggleVisibility":
                var toggle = new TeamsAdaptiveToggleVisibilityAction {
                    Title = title
                };
                if (dictionary.Contains("targetElements") && dictionary["targetElements"] is IEnumerable enumerable && dictionary["targetElements"] is not string) {
                    foreach (var item in enumerable) {
                        var text = item?.ToString();
                        if (!string.IsNullOrWhiteSpace(text)) {
                            toggle.TargetElements.Add(text!);
                        }
                    }
                }
                action = toggle;
                return true;
            case "Action.ShowCard":
                if (dictionary.Contains("card") && dictionary["card"] is IDictionary cardDictionary) {
                    throw new InvalidOperationException(
                        "New-AdaptiveActionSet no longer accepts a dictionary-shaped Action.ShowCard card because silently dropping nested fields is unsafe. " +
                        "Create the nested card with New-AdaptiveAction -Body/-Actions or New-TeamsAdaptiveShowCardAction.");
                }

                action = new TeamsAdaptiveShowCardAction {
                    Title = title,
                    Card = null
                };
                return true;
            default:
                return false;
        }
    }

    private static string? GetDictionaryString(IDictionary dictionary, string key) {
        return TryGetDictionaryValue(dictionary, key, out var value)
            ? value?.ToString()
            : null;
    }

    private static TeamsAdaptiveSubmitAction CreateSubmitAction(IDictionary dictionary, string title) => new() {
        Id = GetDictionaryString(dictionary, "id"),
        Title = title,
        Data = GetDictionaryData(dictionary, "data")
    };

    private static MessageDataValue? GetDictionaryData(IDictionary dictionary, string key) {
        if (!TryGetDictionaryValue(dictionary, key, out var value) || value is null) {
            return null;
        }
        if (value is not IDictionary data) {
            throw new InvalidOperationException($"{key} must be a dictionary-shaped JSON object.");
        }
        return PowerShellMessageDataValueConverter.FromDictionary(data);
    }

    private static bool TryGetDictionaryValue(IDictionary dictionary, string key, out object? value) {
        foreach (DictionaryEntry entry in dictionary) {
            if (string.Equals(entry.Key?.ToString(), key, StringComparison.OrdinalIgnoreCase)) {
                value = entry.Value;
                return true;
            }
        }
        value = null;
        return false;
    }
}
