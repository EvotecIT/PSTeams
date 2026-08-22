using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates Discord embed image or thumbnail media.</summary>
[Cmdlet(VerbsCommon.New, "DiscordImage")]
[Alias("New-DiscordThumbnail")]
[OutputType(typeof(DiscordEmbedMedia))]
public sealed class CmdletNewDiscordImage : PSCmdlet {
    /// <summary>HTTPS or attachment media URI.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public Uri Url { get; set; } = null!;

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordEmbedMedia { Url = Url });
    }
}
