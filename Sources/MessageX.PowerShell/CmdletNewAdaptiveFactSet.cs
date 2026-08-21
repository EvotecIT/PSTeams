using System.Collections;
using System.Management.Automation;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Creates a legacy-named adaptive fact set backed by the MessageX.Teams model.
/// </summary>
[Cmdlet(VerbsCommon.New, "AdaptiveFactSet")]
[OutputType(typeof(TeamsAdaptiveFactSet))]
public sealed class CmdletNewAdaptiveFactSet : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? Facts { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("None", "Small", "Default", "Medium", "Large", "ExtraLarge", "Padding")]
    public string? Spacing { get; set; }

    [Parameter(Mandatory = false)]
    [ValidateSet("Stretch", "Automatic")]
    public string? Height { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Separator { get; set; }

    protected override void ProcessRecord() {
        if (Facts is null) {
            return;
        }

        var factSet = new TeamsAdaptiveFactSet {
            Height = Height,
            Spacing = Spacing,
            Separator = Separator.IsPresent ? true : null
        };

        foreach (var item in Facts.Invoke()) {
            ApplyFact(factSet, item);
        }

        if (factSet.Facts.Count > 0) {
            WriteObject(factSet);
        }
    }

    private static void ApplyFact(TeamsAdaptiveFactSet factSet, object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            return;
        }

        if (value is TeamsAdaptiveFact fact) {
            factSet.Facts.Add(fact);
            return;
        }

        if (value is IDictionary dictionary && TryCreateFact(dictionary, out var converted)) {
            factSet.Facts.Add(converted);
        }
    }

    private static bool TryCreateFact(IDictionary dictionary, out TeamsAdaptiveFact fact) {
        fact = null!;
        var title = GetDictionaryString(dictionary, "title") ?? GetDictionaryString(dictionary, "Title");
        var value = GetDictionaryString(dictionary, "value") ?? GetDictionaryString(dictionary, "Value");
        if (title is null && value is null) {
            return false;
        }

        fact = new TeamsAdaptiveFact {
            Title = title ?? string.Empty,
            Value = value ?? string.Empty
        };
        return true;
    }

    private static string? GetDictionaryString(IDictionary dictionary, string key) {
        if (!dictionary.Contains(key)) {
            return null;
        }

        return dictionary[key]?.ToString();
    }
}
