using System.IO;

namespace MessageX.Slack;

/// <summary>A file uploaded through Slack's external upload workflow.</summary>
public sealed class SlackFileUploadRequest {
    /// <summary>Readable content stream. The caller retains ownership of the stream.</summary>
    public Stream Content { get; set; } = null!;

    /// <summary>Exact number of bytes remaining in <see cref="Content"/>.</summary>
    public long Length { get; set; }

    /// <summary>Provider-visible file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Optional provider-visible title.</summary>
    public string? Title { get; set; }

    /// <summary>Optional screen-reader description for an image.</summary>
    public string? AlternativeText { get; set; }

    /// <summary>Optional Slack snippet syntax identifier.</summary>
    public string? SnippetType { get; set; }

    /// <summary>Optional channel, direct-message, or multiparty-message identifier used to share the file.</summary>
    public string? ConversationId { get; set; }

    /// <summary>Optional parent message timestamp when sharing the file as a reply.</summary>
    public string? ThreadTimestamp { get; set; }

    /// <summary>Optional message text introducing the shared file.</summary>
    public string? InitialComment { get; set; }
}
