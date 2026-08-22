using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a Discord bot channel target.</summary>
[Cmdlet(VerbsCommon.New, "DiscordChannelTarget")]
[OutputType(typeof(DiscordMessageTarget))]
public sealed class CmdletNewDiscordChannelTarget : PSCmdlet {
    /// <summary>Discord channel identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>Optional guild identifier retained in durable references.</summary>
    [Parameter(Mandatory = false)]
    public string? GuildId { get; set; }

    /// <summary>Optional safe display label.</summary>
    [Parameter(Mandatory = false)]
    public string? DisplayName { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(DiscordMessageTarget.ForChannel(ChannelId, GuildId, DisplayName));
    }
}
