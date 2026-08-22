namespace MessageX.Discord;

internal static class DiscordAttachmentReferenceResolver {
    private static readonly HashSet<string> SupportedEmbedExtensions = new(StringComparer.OrdinalIgnoreCase) {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    public static bool TryResolve(
        Uri uri,
        IEnumerable<DiscordAttachment> attachments,
        out DiscordAttachment? resolved) {
        resolved = null;
        if (!string.Equals(uri.Scheme, "attachment", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        var original = uri.OriginalString;
        var schemeSeparator = original.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparator < 0) {
            return false;
        }
        var referenceFileName = original.Substring(schemeSeparator + 3);
        foreach (var attachment in attachments) {
            if (string.Equals(referenceFileName, attachment.FileName, StringComparison.Ordinal)) {
                resolved = attachment;
                return true;
            }
        }
        foreach (var attachment in attachments) {
            if (string.Equals(referenceFileName, attachment.OriginalFileName, StringComparison.Ordinal)) {
                resolved = attachment;
                return true;
            }
        }
        return false;
    }

    public static bool IsSafeEmbedFileName(string fileName) => fileName.All(character =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-' or '.');

    public static bool IsSupportedEmbedFileName(string fileName) =>
        SupportedEmbedExtensions.Contains(Path.GetExtension(fileName));
}
