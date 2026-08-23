using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MessageX.Hosting;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackEventsApiReceiverTests {
    private const string SigningSecret = "test-signing-secret";
    private const string Timestamp = "1787416800";
    private static readonly DateTimeOffset ReceivedAt = DateTimeOffset.FromUnixTimeSeconds(1787416800);

    [Fact]
    public void UrlVerificationReturnsVerifiedJsonChallengeWithoutDispatch() {
        const string json = """
            {"type":"url_verification","challenge":"challenge-123"}
            """;

        var result = Receive(json);

        Assert.Equal(MessageReceiveStatus.Acknowledged, result.Status);
        Assert.Null(result.Envelope);
        Assert.Equal(200, result.Acknowledgement.StatusCode);
        using var response = JsonDocument.Parse(result.Acknowledgement.CopyBody());
        Assert.Equal("challenge-123", response.RootElement.GetProperty("challenge").GetString());
    }

    [Fact]
    public void AppMentionProducesSafeMentionEnvelopeAndRetryMetadata() {
        const string json = """
            {
              "type":"event_callback",
              "team_id":"T01234567",
              "event_id":"Ev01234567",
              "event_time":1787416799,
              "event":{
                "type":"app_mention",
                "user":"U01234567",
                "text":"<@U999> status",
                "channel":"C01234567",
                "ts":"1787416799.000100",
                "event_ts":"1787416799.000100"
              }
            }
            """;

        var result = Receive(json, retryNumber: 2, retryReason: "http_timeout");

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Equal(MessageRouteKind.Mention, result.Route?.Kind);
        Assert.Equal(MessageEventKind.AppMentioned, result.Envelope?.Kind);
        Assert.Equal("Ev01234567", result.Envelope?.DeduplicationKey);
        Assert.Equal("T01234567", result.Envelope?.ScopeId);
        Assert.Equal("U01234567", result.Envelope?.SenderId);
        Assert.Equal("C01234567", result.Envelope?.Conversation?.ConversationId);
        Assert.Equal(MessageConversationKind.Channel, result.Envelope?.Conversation?.ConversationKind);
        Assert.Equal("1787416799.000100", result.Envelope?.Message?.MessageId);
        Assert.Equal(2, result.Envelope?.Payload.RetryNumber);
        Assert.Equal("http_timeout", result.Envelope?.Payload.RetryReason);
        Assert.Equal("app_mention", result.Envelope?.Payload.EventType);
    }

    [Fact]
    public void DirectMessageProducesDirectRouteAndConversationShape() {
        const string json = """
            {
              "type":"event_callback",
              "team_id":"T01234567",
              "event_id":"Ev76543210",
              "event_time":1787416799,
              "event":{
                "type":"message",
                "user":"U01234567",
                "text":"hello",
                "channel":"D01234567",
                "channel_type":"im",
                "ts":"1787416799.000200"
              }
            }
            """;

        var result = Receive(json);

        Assert.Equal(MessageRouteKind.DirectMessage, result.Route?.Kind);
        Assert.Equal(MessageEventKind.MessageReceived, result.Envelope?.Kind);
        Assert.Equal(MessageConversationKind.DirectMessage, result.Envelope?.Conversation?.ConversationKind);
        Assert.Equal("hello", result.Envelope?.Payload.Text);
    }

    [Fact]
    public void MultipartyDirectMessageUsesTheDirectMessageRoute() {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"EvMpim","event":{"type":"message","user":"U1","channel":"G1","channel_type":"mpim","ts":"1787416799.1","text":"group dm"}}
            """;

        var result = Receive(json);

        Assert.Equal(MessageRouteKind.DirectMessage, result.Route?.Kind);
        Assert.Equal(MessageEventKind.MessageReceived, result.Envelope?.Kind);
    }

    [Fact]
    public void UnsupportedVerifiedEventIsAcknowledgedWithoutDispatch() {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"Ev1","event":{"type":"app_home_opened"}}
            """;

        var result = Receive(json);

        Assert.Equal(MessageReceiveStatus.Acknowledged, result.Status);
        Assert.Equal(200, result.Acknowledgement.StatusCode);
        Assert.Null(result.Envelope);
    }

    [Fact]
    public void UnsupportedVerifiedEventDoesNotParseMessageOnlyCoordinateShapes() {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"EvChannel","event":{"type":"channel_created","channel":{"id":"C1","name":"general"}}}
            """;

        var result = Receive(json);

        Assert.Equal(MessageReceiveStatus.Acknowledged, result.Status);
        Assert.Equal(200, result.Acknowledgement.StatusCode);
        Assert.Null(result.Envelope);
    }

    [Fact]
    public void ChangedDeletedReactionAndThreadEventsUseAffectedMessageCoordinates() {
        const string changed = """
            {"type":"event_callback","team_id":"T1","event_id":"EvChanged","event":{"type":"message","subtype":"message_changed","channel":"C1","event_ts":"1787416799.9","message":{"user":"U1","text":"edited","ts":"1787416799.1","thread_ts":"1787416700.1"}}}
            """;
        const string deleted = """
            {"type":"event_callback","team_id":"T1","event_id":"EvDeleted","event":{"type":"message","subtype":"message_deleted","channel":"C1","deleted_ts":"1787416799.2","event_ts":"1787416800.1","previous_message":{"user":"U2","text":"deleted","thread_ts":"1787416700.1"}}}
            """;
        const string reaction = """
            {"type":"event_callback","team_id":"T1","event_id":"EvReaction","event":{"type":"reaction_added","user":"U3","reaction":"thumbsup","event_ts":"1787416801.1","item":{"type":"message","channel":"C2","ts":"1787416799.3"}}}
            """;

        var changedResult = Receive(changed);
        var deletedResult = Receive(deleted);
        var reactionResult = Receive(reaction);

        Assert.Equal("1787416799.1", changedResult.Envelope?.Message?.MessageId);
        Assert.Equal("edited", changedResult.Envelope?.Payload.Text);
        Assert.Equal("U1", changedResult.Envelope?.SenderId);
        Assert.Equal("1787416700.1", changedResult.Envelope?.Message?.ThreadId);
        Assert.Equal(MessageConversationKind.Thread, changedResult.Envelope?.Conversation?.ConversationKind);
        Assert.Equal("1787416799.2", deletedResult.Envelope?.Message?.MessageId);
        Assert.Equal("U2", deletedResult.Envelope?.SenderId);
        Assert.Equal("C2", reactionResult.Envelope?.Conversation?.ConversationId);
        Assert.Equal("1787416799.3", reactionResult.Envelope?.Message?.MessageId);
        Assert.Equal("thumbsup", reactionResult.Envelope?.Payload.ProviderEvent.Reaction);
    }

    [Fact]
    public void SafeEventPayloadRoundTripsWithDefaultSystemTextJson() {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"Ev1","event":{"type":"message","user":"U1","text":"hello","channel":"C1","ts":"1787416799.1"}}
            """;
        var payload = Receive(json).Envelope!.Payload;

        var serialized = JsonSerializer.Serialize(payload);
        var roundTrip = JsonSerializer.Deserialize<SlackInboundEvent>(serialized);

        Assert.Equal("message", roundTrip?.EventType);
        Assert.Equal("C1", roundTrip?.ProviderEvent.ChannelId);
        Assert.DoesNotContain("JsonElement", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedStaleRequestHasDistinctFailureClassification() {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"Ev1","event":{"type":"message","channel":"C1","ts":"1787416799.1"}}
            """;
        var staleRequest = new MessageInboundRequest(
            "installation-1",
            "application/json",
            Encoding.UTF8.GetBytes(json),
            ReceivedAt.AddMinutes(6));

        var result = SlackEventsApiReceiver.Receive(
            staleRequest,
            SigningSecret,
            Sign(json),
            Timestamp);

        Assert.Equal(MessageReceiveFailureKind.Stale, result.FailureKind);
        Assert.Equal(401, result.Acknowledgement.StatusCode);
    }

    [Fact]
    public void InstallationIdentityAcceptsNonEnterpriseAuthorizationWithNullEnterpriseId() {
        const string json = """
            {"type":"event_callback","api_app_id":"A1","team_id":"T-other","event_id":"Ev1",
             "authorizations":[{"team_id":"T1","enterprise_id":null}],
             "event":{"type":"message","user":"U1","channel":"C1","ts":"1787416799.1"}}
            """;

        var result = SlackEventsApiReceiver.Receive(
            Request(json),
            SigningSecret,
            Sign(json),
            Timestamp,
            expectedIdentity: new SlackInstallationIdentity("A1", "T1"));

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Equal("C1", result.Envelope?.Conversation?.ConversationId);
    }

    [Fact]
    public void InvalidSignatureContentTypeAndCoordinatesFailClosed() {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"Ev1","event":{"type":"message","channel":"C1\u000a","ts":"1787416799.1"}}
            """;
        var request = Request(json);

        var badSignature = SlackEventsApiReceiver.Receive(
            request,
            SigningSecret,
            "v0=" + new string('0', 64),
            Timestamp);
        var badType = SlackEventsApiReceiver.Receive(
            new MessageInboundRequest("installation-1", "text/plain", Encoding.UTF8.GetBytes(json), ReceivedAt),
            SigningSecret,
            Sign(json),
            Timestamp);
        var badCoordinates = SlackEventsApiReceiver.Receive(
            request,
            SigningSecret,
            Sign(json),
            Timestamp);
        const string malformedSubtypeJson = """
            {"type":"event_callback","team_id":"T1","event_id":"Ev2","event":{"type":"message","subtype":1,"channel":"C1","ts":"1787416799.1"}}
            """;
        var malformedSubtype = Receive(malformedSubtypeJson);
        const string unsafeTextJson = """
            {"type":"event_callback","team_id":"T1","event_id":"Ev3","event":{"type":"message","user":"U1","channel":"C1","ts":"1787416799.1","text":"hello\u0000world"}}
            """;
        var unsafeText = Receive(unsafeTextJson);

        Assert.Equal(MessageReceiveFailureKind.Unauthorized, badSignature.FailureKind);
        Assert.Equal(401, badSignature.Acknowledgement.StatusCode);
        Assert.Equal(MessageReceiveFailureKind.Unsupported, badType.FailureKind);
        Assert.Equal(415, badType.Acknowledgement.StatusCode);
        Assert.Equal(MessageReceiveFailureKind.Malformed, badCoordinates.FailureKind);
        Assert.Equal(400, badCoordinates.Acknowledgement.StatusCode);
        Assert.Equal(MessageReceiveFailureKind.Malformed, malformedSubtype.FailureKind);
        Assert.Equal(400, malformedSubtype.Acknowledgement.StatusCode);
        Assert.Equal(MessageReceiveFailureKind.Malformed, unsafeText.FailureKind);
        Assert.Equal(400, unsafeText.Acknowledgement.StatusCode);
    }

    private static MessageReceiveResult<SlackInboundEvent> Receive(
        string json,
        int? retryNumber = null,
        string? retryReason = null) =>
        SlackEventsApiReceiver.Receive(
            Request(json),
            SigningSecret,
            Sign(json),
            Timestamp,
            retryNumber,
            retryReason);

    private static MessageInboundRequest Request(string json) => new(
        "installation-1",
        "application/json; charset=utf-8",
        Encoding.UTF8.GetBytes(json),
        ReceivedAt) {
        CorrelationId = "slack-test"
    };

    private static string Sign(string json) {
        var body = Encoding.UTF8.GetBytes(json);
        var prefix = Encoding.UTF8.GetBytes($"v0:{Timestamp}:");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningSecret));
        hmac.TransformBlock(prefix, 0, prefix.Length, null, 0);
        hmac.TransformFinalBlock(body, 0, body.Length);
        return "v0=" + string.Concat(hmac.Hash!.Select(value => value.ToString("x2")));
    }
}
