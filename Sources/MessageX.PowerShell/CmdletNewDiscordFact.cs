using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord embed field.</summary>
[Cmdlet(VerbsCommon.New, "DiscordFact")]
[Alias("New-DiscordField")]
[OutputType(typeof(DiscordEmbedField))]
public sealed class CmdletNewDiscordFact : PSCmdlet {
    /// <summary>Field name.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Field value.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Value { get; set; } = string.Empty;

    /// <summary>Allows this field to share a row with adjacent fields.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter Inline { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordEmbedField { Name = Name, Value = Value, Inline = Inline.IsPresent });
    }
}
