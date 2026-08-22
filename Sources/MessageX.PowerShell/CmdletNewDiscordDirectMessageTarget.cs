using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord bot direct-message target.</summary>
[Cmdlet(VerbsCommon.New, "DiscordDirectMessageTarget")]
[OutputType(typeof(DiscordMessageTarget))]
public sealed class CmdletNewDiscordDirectMessageTarget : PSCmdlet {
    /// <summary>Discord user identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Optional safe display label.</summary>
    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(DiscordMessageTarget.ForDirectMessage(UserId, DisplayName));
    }
}
