namespace MessageX.Teams;

/// <summary>
/// Builds inline data-URL payloads for embedded Teams images.
/// </summary>
public static class TeamsImageDataUtility {
    public static string FromFile(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Image path must not be empty.", nameof(path));
        }

        var bytes = File.ReadAllBytes(path);
        var mediaType = bytes.Length >= 8 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4e && bytes[3] == 0x47 &&
            bytes[4] == 0x0d && bytes[5] == 0x0a && bytes[6] == 0x1a && bytes[7] == 0x0a
                ? "image/png"
                : bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff
                    ? "image/jpeg"
                    : throw new ArgumentException("Inline images must contain PNG or JPEG data.", nameof(path));
        return $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}";
    }
}
