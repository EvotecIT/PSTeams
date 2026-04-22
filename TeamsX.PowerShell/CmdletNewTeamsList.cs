using System.Collections;
using System.Globalization;
using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Builds a legacy list fact from typed list items.
/// </summary>
[Cmdlet(VerbsCommon.New, "TeamsList")]
[Alias("TeamsList")]
[OutputType(typeof(TeamsMessageFact))]
public sealed class CmdletNewTeamsList : PSCmdlet {
    [Parameter(Mandatory = false, Position = 0)]
    public ScriptBlock? List { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public string? Name { get; set; }

    protected override void ProcessRecord() {
        if (List is null) {
            return;
        }

        var lines = new List<string>();
        foreach (var value in List.Invoke()) {
            ApplyListItem(lines, value);
        }

        WriteObject(new TeamsMessageFact {
            Name = Name,
            Value = string.Join("\r", lines)
        });
    }

    private static void ApplyListItem(ICollection<string> lines, object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            return;
        }

        if (value is TeamsMessageListItem listItem) {
            lines.Add(RenderListItem(listItem));
            return;
        }

        if (value is IDictionary dictionary && TryCreateListItem(dictionary, out var converted)) {
            lines.Add(RenderListItem(converted));
        }
    }

    private static string RenderListItem(TeamsMessageListItem item) {
        var marker = item.Numbered ? "1. " : "- ";
        var indent = item.Level > 0 ? new string('\t', item.Level) : string.Empty;
        return string.Concat(indent, marker, item.Text ?? string.Empty);
    }

    private static bool TryCreateListItem(IDictionary dictionary, out TeamsMessageListItem item) {
        item = null!;
        var type = GetDictionaryString(dictionary, "Type");
        if (!string.Equals(type, "ListItem", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        item = new TeamsMessageListItem {
            Text = GetDictionaryString(dictionary, "Text"),
            Level = GetDictionaryInt32(dictionary, "Level"),
            Numbered = GetDictionaryBoolean(dictionary, "Numbered")
        };
        return true;
    }

    private static string? GetDictionaryString(IDictionary dictionary, string key) {
        if (!dictionary.Contains(key)) {
            return null;
        }

        return dictionary[key]?.ToString();
    }

    private static int GetDictionaryInt32(IDictionary dictionary, string key) {
        var value = GetDictionaryString(dictionary, key);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    private static bool GetDictionaryBoolean(IDictionary dictionary, string key) {
        var value = GetDictionaryString(dictionary, key);
        return bool.TryParse(value, out var parsed) && parsed;
    }
}
