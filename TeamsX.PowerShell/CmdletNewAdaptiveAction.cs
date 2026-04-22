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

    private Dictionary<string, object?>? BuildNestedCard() {
        if (Body is null && Actions is null) {
            return null;
        }

        var card = new Dictionary<string, object?> {
            ["type"] = "AdaptiveCard"
        };

        if (Body is not null) {
            card["body"] = Body.Invoke()
                .Select(Unwrap)
                .Where(static value => value is not null)
                .ToArray();
        }

        if (Actions is not null) {
            card["actions"] = Actions.Invoke()
                .Select(Unwrap)
                .Where(static value => value is not null)
                .ToArray();
        }

        return card;
    }

    private static object? Unwrap(object? value) {
        return value is PSObject psObject ? psObject.BaseObject : value;
    }
}
