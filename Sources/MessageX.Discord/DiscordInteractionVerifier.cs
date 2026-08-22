using System.Globalization;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace MessageX.Discord;

/// <summary>Verifies Discord interaction request signatures without exposing cryptography-provider types.</summary>
public static class DiscordInteractionVerifier {
    /// <summary>Verifies an Ed25519 signature over the exact ASCII timestamp followed by the raw request body.</summary>
    public static bool Verify(
        string publicKeyHex,
        string signatureHex,
        string timestamp,
        byte[] requestBody) {
        if (string.IsNullOrWhiteSpace(timestamp) || timestamp.Any(character => character is < '0' or > '9')) {
            return false;
        }
        if (requestBody is null) {
            throw new ArgumentNullException(nameof(requestBody));
        }
        if (!TryDecodeHex(publicKeyHex, 32, out var publicKey) ||
            !TryDecodeHex(signatureHex, 64, out var signature)) {
            return false;
        }

        try {
            var timestampBytes = Encoding.ASCII.GetBytes(timestamp);
            var verifier = new Ed25519Signer();
            verifier.Init(false, new Ed25519PublicKeyParameters(publicKey, 0));
            verifier.BlockUpdate(timestampBytes, 0, timestampBytes.Length);
            verifier.BlockUpdate(requestBody, 0, requestBody.Length);
            return verifier.VerifySignature(signature);
        }
        catch (ArgumentException) {
            return false;
        }
    }

    /// <summary>
    /// Verifies a signature and rejects timestamps outside the supplied age window. Hosting code must
    /// separately reject duplicate signatures because timestamp validation alone does not prevent replay.
    /// </summary>
    public static bool VerifyRecent(
        string publicKeyHex,
        string signatureHex,
        string timestamp,
        byte[] requestBody,
        DateTimeOffset now,
        TimeSpan maximumAge) {
        if (maximumAge <= TimeSpan.Zero ||
            !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds)) {
            return false;
        }
        DateTimeOffset signedAt;
        try {
            signedAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
        catch (ArgumentOutOfRangeException) {
            return false;
        }
        var age = now.ToUniversalTime() - signedAt;
        if (age < -maximumAge || age > maximumAge) {
            return false;
        }
        return Verify(publicKeyHex, signatureHex, timestamp, requestBody);
    }

    private static bool TryDecodeHex(string? value, int expectedBytes, out byte[] bytes) {
        bytes = Array.Empty<byte>();
        if (value is null || value.Length != expectedBytes * 2) {
            return false;
        }
        var decoded = new byte[expectedBytes];
        for (var index = 0; index < decoded.Length; index++) {
            var high = DecodeNibble(value[index * 2]);
            var low = DecodeNibble(value[(index * 2) + 1]);
            if (high < 0 || low < 0) {
                return false;
            }
            decoded[index] = (byte)((high << 4) | low);
        }
        bytes = decoded;
        return true;
    }

    private static int DecodeNibble(char value) {
        if (value is >= '0' and <= '9') {
            return value - '0';
        }
        if (value is >= 'a' and <= 'f') {
            return value - 'a' + 10;
        }
        if (value is >= 'A' and <= 'F') {
            return value - 'A' + 10;
        }
        return -1;
    }
}
