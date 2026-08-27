using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord modal for an immediate interaction response.</summary>
/// <example>
/// <summary>Create a modal with one reason input</summary>
/// <code>$input = New-DiscordTextInput -CustomId 'reason' -Label 'Reason'; $row = New-DiscordActionRow -Components $input; New-DiscordModal -CustomId 'approval' -Title 'Approval' -Components $row</code>
/// </example>
[Cmdlet(VerbsCommon.New, "DiscordModal")]
[OutputType(typeof(DiscordModalRequest))]
public sealed class CmdletNewDiscordModal : PSCmdlet {
    /// <summary>Application-defined modal identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string CustomId { get; set; } = string.Empty;

    /// <summary>User-visible modal title.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Action rows containing one text input each.</summary>
    [Parameter(Mandatory = true, Position = 2)]
    [ValidateCount(1, 5)]
    public DiscordActionRow[] Components { get; set; } = Array.Empty<DiscordActionRow>();

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var modal = new DiscordModalRequest {
            CustomId = CustomId,
            Title = Title
        };
        foreach (var row in Components) {
            if (row is not null) {
                modal.Components.Add(row);
            }
        }
        WriteObject(modal);
    }
}
