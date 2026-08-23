using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MessageX.Discord;

/// <summary>Creates the stable durable identity for a verified Discord interaction.</summary>
internal static class DiscordInteractionIdentity {
    /// <summary>Creates the installation-scoped deduplication key for an interaction.</summary>
    public static string CreateDeduplicationKey(string installationId, string interactionId) {
        byte[] hash;
        using (var sha256 = SHA256.Create()) {
            hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(
                installationId + "\n" + interactionId));
        }
        var builder = new StringBuilder("discord-request:", 16 + (hash.Length * 2));
        foreach (var value in hash) {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }
}
