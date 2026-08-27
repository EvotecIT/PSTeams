using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord action row.</summary>
/// <example>
/// <summary>Create a row containing an approval button</summary>
/// <code>New-DiscordActionRow -Components (New-DiscordButton -Label 'Approve' -CustomId 'approve')</code>
/// </example>
[Cmdlet(VerbsCommon.New, "DiscordActionRow")]
[OutputType(typeof(DiscordActionRow))]
public sealed class CmdletNewDiscordActionRow : PSCmdlet {
    /// <summary>Compatible buttons, one select menu, or one modal text input.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateCount(1, 5)]
    public DiscordInteractiveComponent[] Components { get; set; } = Array.Empty<DiscordInteractiveComponent>();

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var row = new DiscordActionRow();
        foreach (var component in Components) {
            if (component is not null) {
                row.Components.Add(component);
            }
        }
        WriteObject(row);
    }
}
