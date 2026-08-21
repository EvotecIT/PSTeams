using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using MessageX.Teams;

namespace MessageX.PowerShell;

/// <summary>
/// Converts dictionaries and PowerShell objects into Teams sections.
/// </summary>
[Cmdlet(VerbsData.ConvertTo, "TeamsSection")]
[OutputType(typeof(TeamsMessageSection))]
public sealed class CmdletConvertToTeamsSection : PSCmdlet {
    private static readonly Regex SectionTitleRegexValue = new("([A-Z])", RegexOptions.Compiled);

    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public object? InputObject { get; set; }

    [Parameter(Mandatory = false, Position = 1)]
    public string? SectionTitleProperty { get; set; }

    protected override void ProcessRecord() {
        foreach (var value in ExpandInput(InputObject)) {
            if (value is null) {
                continue;
            }

            TeamsMessageSection section;
            try {
                section = BuildSection(value);
            } catch (InvalidDataException exception) {
                ThrowTerminatingError(new ErrorRecord(
                    exception,
                    "InvalidTeamsSectionInput",
                    ErrorCategory.InvalidData,
                    value));
                return;
            }

            WriteObject(section);
        }
    }

    private TeamsMessageSection BuildSection(object value) {
        var section = new TeamsMessageSection();
        foreach (var fact in ConvertFacts(value)) {
            section.Facts.Add(fact);
        }

        var titleProperty = SectionTitleProperty;
        if (!string.IsNullOrWhiteSpace(titleProperty)) {
            var propertyName = titleProperty!;
            var propertyValue = GetPropertyValue(value, propertyName);
            section.ActivityTitle = string.IsNullOrWhiteSpace(propertyValue)
                ? FormatSectionTitle(propertyName)
                : $"{FormatSectionTitle(propertyName)} {propertyValue}";
        }

        return section;
    }

    private static IEnumerable<TeamsMessageFact> ConvertFacts(object value) {
        if (TryGetDictionary(value, out var dictionary)) {
            foreach (DictionaryEntry entry in dictionary) {
                yield return new TeamsMessageFact {
                    Name = entry.Key?.ToString(),
                    Value = entry.Value?.ToString()
                };
            }
            yield break;
        }

        var type = GetInputType(value);
        if (type is not null && CmdletConvertToTeamsFact.IsPrimitiveInput(type)) {
            throw new InvalidDataException("The input is neither a PSObject nor a Hashtable. Operation aborted.");
        }

        foreach (var property in PSObject.AsPSObject(value).Properties) {
            yield return new TeamsMessageFact {
                Name = property.Name,
                Value = property.Value?.ToString()
            };
        }
    }

    private static string? GetPropertyValue(object value, string propertyName) {
        if (TryGetDictionary(value, out var dictionary) && dictionary.Contains(propertyName)) {
            return dictionary[propertyName]?.ToString();
        }

        var property = PSObject.AsPSObject(value).Properties[propertyName];
        return property?.Value?.ToString();
    }

    private static IEnumerable<object?> ExpandInput(object? input) {
        var value = input is PSObject psObject ? psObject.BaseObject : input;
        if (value is null) {
            yield break;
        }

        if (value is IEnumerable enumerable && value is not string && value is not IDictionary) {
            foreach (var entry in enumerable) {
                yield return entry;
            }
            yield break;
        }

        yield return input;
    }

    private static bool TryGetDictionary(object input, out IDictionary dictionary) {
        if (input is IDictionary directDictionary) {
            dictionary = directDictionary;
            return true;
        }

        if (input is PSObject psObject && psObject.BaseObject is IDictionary baseDictionary) {
            dictionary = baseDictionary;
            return true;
        }

        dictionary = null!;
        return false;
    }

    private static Type? GetInputType(object input) {
        if (input is PSObject psObject) {
            return psObject.BaseObject?.GetType() ?? input.GetType();
        }

        return input.GetType();
    }

    private static string FormatSectionTitle(string propertyName) {
        return SectionTitleRegexValue.Replace(propertyName, " $1").Trim();
    }
}
