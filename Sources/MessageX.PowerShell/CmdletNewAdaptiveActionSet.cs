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
        var type = GetDictionaryString(dictionary, "type") ?? GetDictionaryString(dictionary, "Type");
        var title = GetDictionaryString(dictionary, "title") ?? string.Empty;

        switch (type) {
            case "Action.OpenUrl":
                action = new TeamsAdaptiveOpenUrlAction {
                    Title = title,
                    Url = GetDictionaryString(dictionary, "url") ?? string.Empty
                };
                return true;
            case "Action.Submit":
                action = new TeamsAdaptiveSubmitAction {
                    Title = title
                };
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
        if (!dictionary.Contains(key)) {
            return null;
        }

        return dictionary[key]?.ToString();
    }
}
