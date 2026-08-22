namespace MessageX.Discord;

/// <summary>An in-memory file attached to a Discord message.</summary>
public sealed class DiscordAttachment {
    private readonly byte[] _content;

    private DiscordAttachment(
        string fileName,
        byte[] content,
        string? description,
        string? contentType,
        bool isSpoiler) {
        FileName = fileName;
        _content = content;
        Description = description;
        ContentType = contentType;
        IsSpoiler = isSpoiler;
    }

    /// <summary>File name sent to Discord.</summary>
    public string FileName { get; }

    /// <summary>Optional accessible attachment description.</summary>
    public string? Description { get; }

    /// <summary>Optional MIME content type.</summary>
    public string? ContentType { get; }

    /// <summary>Whether Discord should obscure the attachment as a spoiler.</summary>
    public bool IsSpoiler { get; }

    /// <summary>Attachment size in bytes.</summary>
    public int Length => _content.Length;

    internal byte[] Content => _content;

    /// <summary>Creates an attachment from bytes. The supplied content is copied.</summary>
    public static DiscordAttachment FromBytes(
        string fileName,
        byte[] content,
        string? description = null,
        string? contentType = null,
        bool isSpoiler = false) {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Length > 255 ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileName.Contains('/') || fileName.Contains('\\')) {
            throw new ArgumentException("A safe attachment file name is required.", nameof(fileName));
        }
        if (content is null) {
            throw new ArgumentNullException(nameof(content));
        }
        if (description?.Length > 1024) {
            throw new ArgumentException("Discord attachment descriptions cannot exceed 1024 characters.", nameof(description));
        }
        if (!string.IsNullOrWhiteSpace(contentType) &&
            (!contentType!.Contains('/') || contentType.Any(char.IsWhiteSpace))) {
            throw new ArgumentException("Attachment content type must be a MIME media type.", nameof(contentType));
        }

        return new DiscordAttachment(
            fileName.Trim(),
            (byte[])content.Clone(),
            string.IsNullOrWhiteSpace(description) ? null : description,
            string.IsNullOrWhiteSpace(contentType) ? null : contentType!.Trim(),
            isSpoiler);
    }

    /// <summary>Creates an attachment from a local file.</summary>
    public static DiscordAttachment FromFile(
        string path,
        string? description = null,
        string? contentType = null,
        bool isSpoiler = false) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("An attachment path is required.", nameof(path));
        }
        var fullPath = Path.GetFullPath(path);
        return FromBytes(Path.GetFileName(fullPath), File.ReadAllBytes(fullPath), description, contentType, isSpoiler);
    }
}
