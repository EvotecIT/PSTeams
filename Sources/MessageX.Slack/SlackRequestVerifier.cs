using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MessageX.Slack;

/// <summary>Verifies Slack request signatures against the exact raw HTTP request body.</summary>
public static class SlackRequestVerifier {
    private const string SignaturePrefix = "v0=";

    /// <summary>
    /// Verifies an HMAC-SHA256 Slack request signature and rejects timestamps outside the supplied replay window.
    /// </summary>
    public static bool VerifyRecent(
        string signingSecret,
        string signature,
        string timestamp,
        byte[] requestBody,
        DateTimeOffset now,
        TimeSpan maximumAge) {
        return VerifyRecentDetailed(
            signingSecret,
            signature,
            timestamp,
            requestBody,
            now,
            maximumAge) == SlackRequestVerificationResult.Valid;
    }

    /// <summary>Verifies a Slack signature and distinguishes a valid stale request from an invalid signature.</summary>
    public static SlackRequestVerificationResult VerifyRecentDetailed(
        string signingSecret,
        string signature,
        string timestamp,
        byte[] requestBody,
        DateTimeOffset now,
        TimeSpan maximumAge) {
        if (requestBody is null) {
            throw new ArgumentNullException(nameof(requestBody));
        }
        if (string.IsNullOrEmpty(signingSecret) ||
            maximumAge <= TimeSpan.Zero ||
            !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds) ||
            !TryDecodeSignature(signature, out var suppliedSignature)) {
            return SlackRequestVerificationResult.Invalid;
        }

        DateTimeOffset signedAt;
        try {
            signedAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
        catch (ArgumentOutOfRangeException) {
            return SlackRequestVerificationResult.Invalid;
        }

        var prefix = Encoding.UTF8.GetBytes($"v0:{timestamp}:");
        var signedBytes = new byte[prefix.Length + requestBody.Length];
        Buffer.BlockCopy(prefix, 0, signedBytes, 0, prefix.Length);
        Buffer.BlockCopy(requestBody, 0, signedBytes, prefix.Length, requestBody.Length);
        byte[] expectedSignature;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret))) {
            expectedSignature = hmac.ComputeHash(signedBytes);
        }
        if (!FixedTimeEquals(expectedSignature, suppliedSignature)) {
            return SlackRequestVerificationResult.Invalid;
        }

        var age = now.ToUniversalTime() - signedAt;
        return age < -maximumAge || age > maximumAge
            ? SlackRequestVerificationResult.Stale
            : SlackRequestVerificationResult.Valid;
    }

    private static bool TryDecodeSignature(string? value, out byte[] bytes) {
        bytes = Array.Empty<byte>();
        if (value is null ||
            value.Length != SignaturePrefix.Length + 64 ||
            !value.StartsWith(SignaturePrefix, StringComparison.Ordinal)) {
            return false;
        }

        var decoded = new byte[32];
        for (var index = 0; index < decoded.Length; index++) {
            var high = DecodeNibble(value[SignaturePrefix.Length + (index * 2)]);
            var low = DecodeNibble(value[SignaturePrefix.Length + (index * 2) + 1]);
            if (high < 0 || low < 0) {
                return false;
            }
            decoded[index] = (byte)((high << 4) | low);
        }
        bytes = decoded;
        return true;
    }

    private static bool FixedTimeEquals(byte[] left, byte[] right) {
        if (left.Length != right.Length) {
            return false;
        }
        var difference = 0;
        for (var index = 0; index < left.Length; index++) {
            difference |= left[index] ^ right[index];
        }
        return difference == 0;
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
