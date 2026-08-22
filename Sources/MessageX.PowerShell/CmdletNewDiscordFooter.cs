using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates Discord embed footer metadata.</summary>
[Cmdlet(VerbsCommon.New, "DiscordFooter")]
[OutputType(typeof(DiscordEmbedFooter))]
public sealed class CmdletNewDiscordFooter : PSCmdlet {
    /// <summary>Footer text.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional footer icon.</summary>
    [Parameter(Mandatory = false)]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(new DiscordEmbedFooter { Text = Text, IconUrl = IconUrl });
    }
}
