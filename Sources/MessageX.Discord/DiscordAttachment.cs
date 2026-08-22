using System.Net.Http.Headers;

namespace MessageX.Discord;

/// <summary>An in-memory file attached to a Discord message.</summary>
public sealed class DiscordAttachment {
    private readonly byte[] _content;

    private DiscordAttachment(
        string originalFileName,
        string fileName,
        byte[] content,
        string? description,
        string? contentType,
        bool isSpoiler) {
        OriginalFileName = originalFileName;
        FileName = fileName;
        _content = content;
        Description = description;
        ContentType = contentType;
        IsSpoiler = isSpoiler;
    }

    /// <summary>File name sent to Discord.</summary>
    public string FileName { get; }

    internal string OriginalFileName { get; }

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
            fileName.Any(char.IsControl) ||
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
        var normalizedContentType = NormalizeContentType(contentType);

        var originalFileName = fileName.Trim();
        var effectiveSpoiler = isSpoiler || originalFileName.StartsWith("SPOILER_", StringComparison.Ordinal);
        var uploadFileName = effectiveSpoiler && !originalFileName.StartsWith("SPOILER_", StringComparison.Ordinal)
            ? "SPOILER_" + originalFileName
            : originalFileName;
        if (uploadFileName.Length > 255) {
            throw new ArgumentException("A spoiler attachment file name cannot exceed 247 characters before its prefix.", nameof(fileName));
        }

        return new DiscordAttachment(
            originalFileName,
            uploadFileName,
            (byte[])content.Clone(),
            string.IsNullOrWhiteSpace(description) ? null : description,
            normalizedContentType,
            effectiveSpoiler);
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
        return FromBytes(Path.GetFileName(fullPath), ReadBoundedFile(fullPath), description, contentType, isSpoiler);
    }

    private static byte[] ReadBoundedFile(string fullPath) {
        using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length > DiscordMessageValidator.MaximumAttachmentBytes) {
            throw new ArgumentException(
                $"Discord attachments cannot exceed {DiscordMessageValidator.MaximumAttachmentBytes} bytes.",
                nameof(fullPath));
        }

        var content = new byte[(int)stream.Length];
        var offset = 0;
        while (offset < content.Length) {
            var read = stream.Read(content, offset, content.Length - offset);
            if (read == 0) {
                Array.Resize(ref content, offset);
                return content;
            }
            offset += read;
        }
        if (stream.ReadByte() != -1) {
            throw new ArgumentException(
                $"Discord attachments cannot exceed {DiscordMessageValidator.MaximumAttachmentBytes} bytes.",
                nameof(fullPath));
        }
        return content;
    }

    private static string? NormalizeContentType(string? contentType) {
        if (string.IsNullOrWhiteSpace(contentType)) {
            return null;
        }
        try {
            var parsed = MediaTypeHeaderValue.Parse(contentType!.Trim()) ??
                throw new FormatException("A MIME media type is required.");
            if (parsed.Parameters.Any(parameter =>
                string.IsNullOrWhiteSpace(parameter.Name) || string.IsNullOrWhiteSpace(parameter.Value))) {
                throw new FormatException("MIME parameters require both a name and value.");
            }
            return parsed.ToString();
        }
        catch (FormatException exception) {
            throw new ArgumentException("Attachment content type must be a valid MIME media type.", nameof(contentType), exception);
        }
    }
}
