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
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
