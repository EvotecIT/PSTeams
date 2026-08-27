using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord string-select option.</summary>
/// <example>
/// <summary>Create an option for a production environment</summary>
/// <code>New-DiscordSelectOption -Label 'Production' -Value 'prod' -Description 'Deploy to production'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "DiscordSelectOption")]
[OutputType(typeof(DiscordSelectOption))]
public sealed class CmdletNewDiscordSelectOption : PSCmdlet {
    /// <summary>User-visible label.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Application-defined value.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Value { get; set; } = string.Empty;

    /// <summary>Optional user-visible description.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Selects this option by default.</summary>
    [Parameter]
    public SwitchParameter Default { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordSelectOption {
            Label = Label,
            Value = Value,
            Description = Description,
            Default = Default.IsPresent
        });
    }
}
