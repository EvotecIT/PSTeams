using System.Text;
using MessageX.Discord;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace MessageX.Tests;

public sealed class DiscordInteractionVerifierTests {
    [Fact]
    public void FixedIndependentEd25519VectorIsAccepted() {
        const string publicKey = "79B5562E8FE654F94078B112E8A98BA7901F853AE695BED7E0E3910BAD049664";
        const string signature = "7FB1DF4757DE1D90C78D7AC9CAA2FD7B00B003DAFB9E49C24C49833E71559EC9" +
                                 "5694335976D724D0A98DC692B400D271AAF043036F35540523D6CF35F2D56F07";
        const string timestamp = "1787301000";
        var body = Encoding.UTF8.GetBytes("{\"type\":1}");

        Assert.True(DiscordInteractionVerifier.Verify(publicKey, signature, timestamp, body));
    }

    [Fact]
    public void ValidSignatureIsAcceptedAndTamperingIsRejected() {
        const string timestamp = "1787301000";
        var body = Encoding.UTF8.GetBytes("{\"type\":1}");
        var material = CreateSignature(timestamp, body);

        Assert.True(DiscordInteractionVerifier.Verify(material.PublicKeyHex, material.SignatureHex, timestamp, body));
        Assert.False(DiscordInteractionVerifier.Verify(
            material.PublicKeyHex,
            material.SignatureHex,
            timestamp,
            Encoding.UTF8.GetBytes("{\"type\":2}")));
        Assert.False(DiscordInteractionVerifier.Verify(material.PublicKeyHex, material.SignatureHex, "not-time", body));
    }

    [Fact]
    public void ReplayWindowRejectsOldAndFutureRequests() {
        const string timestamp = "1787301000";
        var body = Encoding.UTF8.GetBytes("{\"type\":1}");
        var material = CreateSignature(timestamp, body);
        var signedAt = DateTimeOffset.FromUnixTimeSeconds(1787301000);

        Assert.True(DiscordInteractionVerifier.VerifyRecent(
            material.PublicKeyHex,
            material.SignatureHex,
            timestamp,
            body,
            signedAt.AddMinutes(2),
            TimeSpan.FromMinutes(5)));
        Assert.False(DiscordInteractionVerifier.VerifyRecent(
            material.PublicKeyHex,
            material.SignatureHex,
            timestamp,
            body,
            signedAt.AddMinutes(6),
            TimeSpan.FromMinutes(5)));
        Assert.False(DiscordInteractionVerifier.VerifyRecent(
            material.PublicKeyHex,
            material.SignatureHex,
            timestamp,
            body,
            signedAt.AddMinutes(-6),
            TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void MalformedKeyAndSignatureAreRejectedWithoutThrowing() {
        Assert.False(DiscordInteractionVerifier.Verify("bad", new string('0', 128), "1", Array.Empty<byte>()));
        Assert.False(DiscordInteractionVerifier.Verify(new string('0', 64), "bad", "1", Array.Empty<byte>()));
        Assert.False(DiscordInteractionVerifier.Verify(
            new string('0', 64),
            new string('0', 128),
            "1",
            Array.Empty<byte>()));
    }

    private static SignatureMaterial CreateSignature(string timestamp, byte[] body) {
        var seed = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();
        var privateKey = new Ed25519PrivateKeyParameters(seed, 0);
        var timestampBytes = Encoding.ASCII.GetBytes(timestamp);
        var signer = new Ed25519Signer();
        signer.Init(true, privateKey);
        signer.BlockUpdate(timestampBytes, 0, timestampBytes.Length);
        signer.BlockUpdate(body, 0, body.Length);
        return new SignatureMaterial(
            Convert.ToHexString(privateKey.GeneratePublicKey().GetEncoded()),
            Convert.ToHexString(signer.GenerateSignature()));
    }

    private sealed record SignatureMaterial(string PublicKeyHex, string SignatureHex);
}
