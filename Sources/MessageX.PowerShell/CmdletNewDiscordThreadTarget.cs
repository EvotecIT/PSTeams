using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord bot thread-channel target.</summary>
[Cmdlet(VerbsCommon.New, "DiscordThreadTarget")]
[OutputType(typeof(DiscordMessageTarget))]
public sealed class CmdletNewDiscordThreadTarget : PSCmdlet {
    /// <summary>Discord thread-channel identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>Optional guild identifier retained in durable references.</summary>
    [Parameter(Mandatory = false)]
    public string? GuildId { get; set; }

    /// <summary>Optional safe display label.</summary>
    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(DiscordMessageTarget.ForThread(ThreadId, GuildId, DisplayName));
    }
}
