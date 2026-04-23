using System.Collections;
using System.IO;
using System.Management.Automation;
using TeamsX;

namespace TeamsX.PowerShell;

/// <summary>
/// Converts dictionaries and PowerShell objects into Teams facts.
/// </summary>
[Cmdlet(VerbsData.ConvertTo, "TeamsFact")]
[OutputType(typeof(TeamsMessageFact))]
public sealed class CmdletConvertToTeamsFact : PSCmdlet {
    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public object? InputObject { get; set; }

    protected override void ProcessRecord() {
        foreach (var value in ExpandInput(InputObject)) {
            ConvertInput(value);
        }
    }

    private void ConvertInput(object? input) {
        if (input is null) {
            return;
        }

        if (TryGetDictionary(input, out var dictionary)) {
            foreach (DictionaryEntry entry in dictionary) {
                WriteFact(entry.Key?.ToString(), entry.Value?.ToString());
            }
            return;
        }

        var type = GetInputType(input);
        if (type is not null && IsPrimitiveInput(type)) {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidDataException("The input is neither a PSObject nor a Hashtable. Operation aborted."),
                "InvalidTeamsFactInput",
                ErrorCategory.InvalidData,
                input));
        }

        var properties = PSObject.AsPSObject(input).Properties;
        foreach (var property in properties) {
            WriteFact(property.Name, property.Value?.ToString());
        }
    }

    private void WriteFact(string? name, string? value) {
        WriteObject(new TeamsMessageFact {
            Name = name,
            Value = value
        });
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

    internal static bool IsPrimitiveInput(Type type) {
        if (type.IsEnum || type.IsPrimitive) {
            return true;
        }

        return type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Uri)
            || type == typeof(byte[]);
    }
}
