using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Creates an in-memory Discord attachment from a file or byte array.</summary>
[Cmdlet(VerbsCommon.New, "DiscordAttachment", DefaultParameterSetName = "Path")]
[OutputType(typeof(DiscordAttachment))]
public sealed class CmdletNewDiscordAttachment : PSCmdlet {
    /// <summary>Local file path.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>Attachment bytes.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Bytes")]
    public byte[] Bytes { get; set; } = Array.Empty<byte>();

    /// <summary>File name used with the byte-array parameter set.</summary>
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Bytes")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Optional accessible attachment description.</summary>
    [Parameter(Mandatory = false)]
    public string? Description { get; set; }

    /// <summary>Optional MIME content type.</summary>
    [Parameter(Mandatory = false)]
    public string? ContentType { get; set; }

    /// <summary>Marks the attachment as a spoiler.</summary>
    [Parameter(Mandatory = false)]
    public SwitchParameter Spoiler { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(ParameterSetName == "Bytes"
            ? DiscordAttachment.FromBytes(FileName, Bytes, Description, ContentType, Spoiler.IsPresent)
            : DiscordAttachment.FromFile(GetUnresolvedProviderPathFromPSPath(Path), Description, ContentType, Spoiler.IsPresent));
    }
}
