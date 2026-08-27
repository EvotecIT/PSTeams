using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord string select menu.</summary>
/// <example>
/// <summary>Create an environment selector</summary>
/// <code>$option = New-DiscordSelectOption -Label 'Production' -Value 'prod'; New-DiscordStringSelect -CustomId 'environment' -Options $option -Placeholder 'Choose an environment'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "DiscordStringSelect")]
[OutputType(typeof(DiscordStringSelect))]
public sealed class CmdletNewDiscordStringSelect : PSCmdlet {
    /// <summary>Application-defined identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string CustomId { get; set; } = string.Empty;

    /// <summary>Selectable options.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    [ValidateCount(1, 25)]
    public DiscordSelectOption[] Options { get; set; } = Array.Empty<DiscordSelectOption>();

    /// <summary>Placeholder shown before selection.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Minimum number of values.</summary>
    [Parameter]
    [ValidateRange(0, 25)]
    public int MinimumValues { get; set; } = 1;

    /// <summary>Maximum number of values.</summary>
    [Parameter]
    [ValidateRange(1, 25)]
    public int MaximumValues { get; set; } = 1;

    /// <summary>Creates a disabled select menu.</summary>
    [Parameter]
    public SwitchParameter Disabled { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var select = new DiscordStringSelect {
            CustomId = CustomId,
            Placeholder = Placeholder,
            MinimumValues = MinimumValues,
            MaximumValues = MaximumValues,
            Disabled = Disabled.IsPresent
        };
        foreach (var option in Options) {
            if (option is not null) {
                select.Options.Add(option);
            }
        }
        WriteObject(select);
    }
}
