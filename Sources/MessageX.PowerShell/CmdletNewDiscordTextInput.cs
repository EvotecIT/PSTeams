using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord modal text input.</summary>
/// <example>
/// <summary>Create a multiline reason input</summary>
/// <code>New-DiscordTextInput -CustomId 'reason' -Label 'Reason' -Style Paragraph -MaximumLength 500 -Placeholder 'Explain the change'</code>
/// </example>
[Cmdlet(VerbsCommon.New, "DiscordTextInput")]
[OutputType(typeof(DiscordTextInput))]
public sealed class CmdletNewDiscordTextInput : PSCmdlet {
    /// <summary>Application-defined identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string CustomId { get; set; } = string.Empty;

    /// <summary>User-visible label.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Input layout style.</summary>
    [Parameter]
    public DiscordTextInputStyle Style { get; set; } = DiscordTextInputStyle.Short;

    /// <summary>Minimum accepted length.</summary>
    [Parameter]
    [ValidateRange(0, 4000)]
    public int? MinimumLength { get; set; }

    /// <summary>Maximum accepted length.</summary>
    [Parameter]
    [ValidateRange(1, 4000)]
    public int? MaximumLength { get; set; }

    /// <summary>Allows an empty value.</summary>
    [Parameter]
    public SwitchParameter Optional { get; set; }

    /// <summary>Prepopulated value.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>Placeholder for an empty input.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordTextInput {
            CustomId = CustomId,
            Label = Label,
            Style = Style,
            MinimumLength = MinimumLength,
            MaximumLength = MaximumLength,
            Required = !Optional,
            Value = Value,
            Placeholder = Placeholder
        });
    }
}
