using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates a rich Discord embed.</summary>
[Cmdlet(VerbsCommon.New, "DiscordSection")]
[Alias("New-DiscordEmbed")]
[OutputType(typeof(DiscordEmbed))]
public sealed class CmdletNewDiscordSection : PSCmdlet {
    /// <summary>Optional embed title.</summary>
    [Parameter(Mandatory = false, Position = 0)]
    public string? Title { get; set; }

    /// <summary>Optional embed description.</summary>
    [Parameter(Mandatory = false, Position = 1)]
    public string? Description { get; set; }

    /// <summary>Optional link applied to the title.</summary>
    [Parameter(Mandatory = false)]
    public Uri? Url { get; set; }

    /// <summary>Optional 24-bit RGB color.</summary>
    [Parameter(Mandatory = false)]
    [ValidateRange(0, 0xFFFFFF)]
    public int? Color { get; set; }

    /// <summary>Optional timestamp.</summary>
    [Parameter(Mandatory = false)]
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>Optional author metadata.</summary>
    [Parameter(Mandatory = false)]
    public DiscordEmbedAuthor? Author { get; set; }

    /// <summary>Optional footer metadata.</summary>
    [Parameter(Mandatory = false)]
    public DiscordEmbedFooter? Footer { get; set; }

    /// <summary>Optional large image.</summary>
    [Parameter(Mandatory = false)]
    public DiscordEmbedMedia? Image { get; set; }

    /// <summary>Optional compact thumbnail.</summary>
    [Parameter(Mandatory = false)]
    public DiscordEmbedMedia? Thumbnail { get; set; }

    /// <summary>Optional embed fields.</summary>
    [Parameter(Mandatory = false)]
    public DiscordEmbedField[] Fields { get; set; } = Array.Empty<DiscordEmbedField>();

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var embed = new DiscordEmbed {
            Title = Title,
            Description = Description,
            Url = Url,
            Color = Color,
            Timestamp = Timestamp,
            Author = Author,
            Footer = Footer,
            Image = Image,
            Thumbnail = Thumbnail
        };
        foreach (var field in Fields ?? Array.Empty<DiscordEmbedField>()) {
            if (field is not null) {
                embed.Fields.Add(field);
            }
        }
        WriteObject(embed);
    }
}
