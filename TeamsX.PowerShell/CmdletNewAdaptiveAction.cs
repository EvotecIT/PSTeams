using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive action backed by the TeamsX model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveAction")]
[OutputType(typeof(TeamsAdaptiveAction))]
public sealed class CmdletNewAdaptiveAction : PSCmdlet {
    [Parameter(Mandatory = false)]
    public ScriptBlock? Body { get; set; }

    [Parameter(Mandatory = false)]
    public ScriptBlock? Actions { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Action.ShowCard", "Action.Submit", "Action.OpenUrl", "Action.ToggleVisibility")]
    public string Type { get; set; } = "Action.ShowCard";

    [Parameter(Mandatory = false)]
    public string? ActionUrl { get; set; }

    [Parameter(Mandatory = false)]
    public string? Title { get; set; }

    protected override void ProcessRecord() {
        if (!string.IsNullOrWhiteSpace(ActionUrl) ||
            string.Equals(Type, "Action.OpenUrl", StringComparison.OrdinalIgnoreCase)) {
            WriteObject(new TeamsAdaptiveOpenUrlAction {
                Title = Title ?? string.Empty,
                Url = ActionUrl ?? string.Empty
            });
            return;
        }

        if (string.Equals(Type, "Action.Submit", StringComparison.OrdinalIgnoreCase)) {
            WriteObject(new TeamsAdaptiveSubmitAction {
                Title = Title ?? string.Empty
            });
            return;
        }

        if (string.Equals(Type, "Action.ToggleVisibility", StringComparison.OrdinalIgnoreCase)) {
            WriteObject(new TeamsAdaptiveToggleVisibilityAction {
                Title = Title ?? string.Empty
            });
            return;
        }

        var card = BuildNestedCard();
        WriteObject(new TeamsAdaptiveShowCardAction {
            Title = Title ?? string.Empty,
            Card = card
        });
    }

    private TeamsAdaptiveCard? BuildNestedCard() {
        if (Body is null && Actions is null) {
            return null;
        }

        var card = new TeamsAdaptiveCard();

        if (Body is not null) {
            foreach (var value in Body.Invoke().Select(Unwrap)) {
                if (value is TeamsAdaptiveCardElement element) {
                    card.Body.Add(element);
                } else if (value is TeamsAdaptiveMention mention) {
                    card.Mentions.Add(mention);
                } else if (value is not null) {
                    throw CreateLegacyInputMigrationException("Body", value);
                }
            }
        }

        if (Actions is not null) {
            foreach (var value in Actions.Invoke().Select(Unwrap)) {
                if (value is TeamsAdaptiveAction action) {
                    card.Actions.Add(action);
                } else if (value is not null) {
                    throw CreateLegacyInputMigrationException("Actions", value);
                }
            }
        }

        return card;
    }

    private static object? Unwrap(object? value) {
        return value is PSObject psObject ? psObject.BaseObject : value;
    }

    private static InvalidOperationException CreateLegacyInputMigrationException(string parameterName, object value) {
        return new InvalidOperationException(
            $"New-AdaptiveAction -{parameterName} no longer accepts untyped or dictionary-shaped Adaptive Card content ({value.GetType().Name}) because silently dropping nested fields is unsafe. " +
            "Build typed content with New-AdaptiveTextBlock, New-AdaptiveImage, New-AdaptiveAction, and the other New-Adaptive* commands.");
    }
}
