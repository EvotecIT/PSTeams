using System.Text;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackRequestVerifierTests {
    private const string OfficialSigningSecret = "8f742231b10e8888abcd99yyyzzz85a5";
    private const string OfficialTimestamp = "1531420618";
    private const string OfficialSignature = "v0=a2114d57b48eac39b9ad189dd8316235a7b4a8d21a10bd27519666489c69b503";
    private const string OfficialBody = "token=xyzz0WbapA4vBCDEFasx0q6G&team_id=T1DC2JH3J&team_domain=testteamnow&channel_id=G8PSS9T3V&channel_name=foobar&user_id=U2CERLKJA&user_name=roadrunner&command=%2Fwebhook-collect&text=&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT1DC2JH3J%2F397700885554%2F96rGlfmibIGlgcZRskXaIFfN&trigger_id=398738663015.47445629121.803a0bc887a14d10d2c447fce8b6703c";

    [Fact]
    public void VerifyRecentAcceptsOfficialRawBodySignatureExample() {
        var signedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(OfficialTimestamp));

        var verified = SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            OfficialSignature,
            OfficialTimestamp,
            Encoding.UTF8.GetBytes(OfficialBody),
            signedAt.AddMinutes(4),
            TimeSpan.FromMinutes(5));

        Assert.True(verified);
    }

    [Fact]
    public void VerifyRecentRejectsBodyMutationStaleTimestampAndMalformedSignature() {
        var signedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(OfficialTimestamp));
        var body = Encoding.UTF8.GetBytes(OfficialBody);

        Assert.False(SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            OfficialSignature,
            OfficialTimestamp,
            body.Concat(new byte[] { (byte)' ' }).ToArray(),
            signedAt,
            TimeSpan.FromMinutes(5)));
        Assert.False(SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            OfficialSignature,
            OfficialTimestamp,
            body,
            signedAt.AddMinutes(6),
            TimeSpan.FromMinutes(5)));
        Assert.False(SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            "v1=" + OfficialSignature.Substring(3),
            OfficialTimestamp,
            body,
            signedAt,
            TimeSpan.FromMinutes(5)));
        Assert.False(SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            OfficialSignature,
            "not-a-timestamp",
            body,
            signedAt,
            TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void VerifyRecentRejectsFutureReplayAndRequiresRawBody() {
        var signedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(OfficialTimestamp));
        var body = Encoding.UTF8.GetBytes(OfficialBody);

        Assert.False(SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            OfficialSignature,
            OfficialTimestamp,
            body,
            signedAt.AddMinutes(-6),
            TimeSpan.FromMinutes(5)));
        Assert.Throws<ArgumentNullException>(() => SlackRequestVerifier.VerifyRecent(
            OfficialSigningSecret,
            OfficialSignature,
            OfficialTimestamp,
            null!,
            signedAt,
            TimeSpan.FromMinutes(5)));
    }
}
