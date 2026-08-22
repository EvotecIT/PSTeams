using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates Discord embed author metadata.</summary>
[Cmdlet(VerbsCommon.New, "DiscordAuthor")]
[OutputType(typeof(DiscordEmbedAuthor))]
public sealed class CmdletNewDiscordAuthor : PSCmdlet {
    /// <summary>Author name.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional author link.</summary>
    [Parameter(Mandatory = false)]
    public Uri? Url { get; set; }

    /// <summary>Optional author icon.</summary>
    [Parameter(Mandatory = false)]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordEmbedAuthor { Name = Name, Url = Url, IconUrl = IconUrl });
    }
}
