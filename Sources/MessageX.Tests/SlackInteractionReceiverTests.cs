using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MessageX.Hosting;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackInteractionReceiverTests {
    private const string SigningSecret = "test-signing-secret";
    private const string Timestamp = "1787418600";
    private static readonly DateTimeOffset ReceivedAt = DateTimeOffset.FromUnixTimeSeconds(1787418600);

    [Fact]
    public void SlashCommandProducesNamedRouteAndKeepsCapabilitiesTransient() {
        const string body = "command=%2Fstatus&user_id=U123&team_id=T123&channel_id=C123&text=hello+world&trigger_id=trigger-1&response_url=https%3A%2F%2Fhooks.slack.com%2Fcommands%2FT123%2F1%2Fsecret";

        var result = Receive(body);

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Equal(MessageRouteKind.Command, result.Route?.Kind);
        Assert.Equal("status", result.Route?.Name);
        Assert.Equal(MessageEventKind.CommandInvoked, result.Envelope?.Kind);
        Assert.Equal("hello world", result.Envelope?.Payload.Text);
        Assert.Equal("trigger-1", result.Envelope?.Payload.TransientContext.TriggerId);
        Assert.Contains("hooks.slack.com", result.Envelope?.Payload.TransientContext.ResponseUrl);
        Assert.StartsWith("slack-request:", result.Envelope?.DeduplicationKey);
        Assert.DoesNotContain("secret", result.Envelope?.DeduplicationKey);
        Assert.Equal("C123", result.Envelope?.Conversation?.ConversationId);
        Assert.Equal("installation-1", result.Envelope?.Conversation?.InstallationId);
        var persistedShape = JsonSerializer.Serialize(result.Envelope?.Payload);
        Assert.DoesNotContain("response_url", persistedShape, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("trigger-1", persistedShape, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", persistedShape, StringComparison.Ordinal);
    }

    [Fact]
    public void BlockActionUsesActionIdAndSafeMessageCoordinates() {
        const string payload = """
            {
              "type":"block_actions",
              "team":{"id":"T123"},
              "user":{"id":"U123"},
              "channel":{"id":"C123"},
              "container":{"channel_id":"C123","message_ts":"1787418599.000100"},
              "trigger_id":"trigger-2",
              "response_url":"https://hooks.slack.com/actions/T123/1/secret",
              "actions":[{"type":"button","action_id":"approve","value":"yes"}]
            }
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageRouteKind.Action, result.Route?.Kind);
        Assert.Equal("approve", result.Route?.Name);
        Assert.Equal(SlackInteractionKind.BlockAction, result.Envelope?.Payload.Kind);
        Assert.Equal("1787418599.000100", result.Envelope?.Message?.MessageId);
        Assert.Equal("button", result.Envelope?.Payload.ProviderPayload?.Actions[0].Type);
        Assert.Equal("yes", result.Envelope?.Payload.ProviderPayload?.Actions[0].Value);
        Assert.Equal("trigger-2", result.Envelope?.Payload.TransientContext.TriggerId);
    }

    [Fact]
    public void ViewSubmissionUsesDistinctSubmissionClassification() {
        const string payload = """
            {
              "type":"view_submission",
              "team":{"id":"T123"},
              "user":{"id":"U123"},
              "view":{"callback_id":"approval","state":{"values":{}}}
            }
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageRouteKind.Submission, result.Route?.Kind);
        Assert.Equal("approval", result.Route?.Name);
        Assert.Equal(MessageEventKind.ModalSubmitted, result.Envelope?.Kind);
        Assert.Equal(SlackInteractionKind.ViewSubmission, result.Envelope?.Payload.Kind);
    }

    [Fact]
    public void MessageShortcutPreservesSelectedMessageCoordinates() {
        const string payload = """
            {
              "type":"message_action",
              "callback_id":"inspect",
              "team":{"id":"T123"},
              "user":{"id":"U123"},
              "channel":{"id":"C123"},
              "message":{"ts":"1787418599.000200","text":"selected"}
            }
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal("1787418599.000200", result.Envelope?.Message?.MessageId);
        Assert.Equal("selected", result.Envelope?.Payload.ProviderPayload?.Message?.Text);
    }

    [Fact]
    public void TypedInteractionPayloadRoundTripsWithoutTransientCapabilities() {
        const string payload = """
            {
              "type":"view_submission",
              "team":{"id":"T123"},
              "user":{"id":"U123"},
              "trigger_id":"secret-trigger",
              "view":{"callback_id":"approval","state":{"values":{"block":{"choice":{"type":"static_select","selected_option":{"value":"yes"}}}}}}
            }
            """;
        var result = Receive(PayloadForm(payload));

        var json = JsonSerializer.Serialize(result.Envelope!.Payload);
        var roundTrip = JsonSerializer.Deserialize<SlackInteractionEvent>(json);

        Assert.Equal("yes", roundTrip?.ProviderPayload?.View?.Values[0].SelectedValues[0]);
        Assert.Null(roundTrip?.TransientContext.TriggerId);
        Assert.DoesNotContain("secret-trigger", json, StringComparison.Ordinal);
    }

    [Fact]
    public void UnsupportedVerifiedInteractionIsAcknowledgedWithoutDispatch() {
        const string payload = """
            {"type":"workflow_step_edit","team":{"id":"T123"},"user":{"id":"U123"}}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageReceiveStatus.Acknowledged, result.Status);
        Assert.Equal(200, result.Acknowledgement.StatusCode);
        Assert.Null(result.Envelope);
    }

    [Theory]
    [InlineData("command=%ZZstatus&user_id=U1")]
    [InlineData("command=%2Fstatus&command=%2Fother&user_id=U1")]
    [InlineData("command=%2Fstatus&user_id=U1%0A")]
    [InlineData("payload=%7Bnot-json%7D")]
    public void MalformedFormAndCoordinatesAreRejectedAfterVerification(string body) {
        var result = Receive(body);

        Assert.Equal(MessageReceiveFailureKind.Malformed, result.FailureKind);
        Assert.Equal(400, result.Acknowledgement.StatusCode);
    }

    [Fact]
    public void MultipleActionsAndWrongNestedTypesFailClosed() {
        const string multipleActions = """
            {"type":"block_actions","team":{"id":"T1"},"user":{"id":"U1"},"actions":[{"action_id":"a"},{"action_id":"b"}]}
            """;
        const string wrongUser = """
            {"type":"block_actions","team":{"id":"T1"},"user":"U1","actions":[{"action_id":"a"}]}
            """;

        var multiple = Receive(PayloadForm(multipleActions));
        var wrong = Receive(PayloadForm(wrongUser));

        Assert.Equal(MessageReceiveFailureKind.Malformed, multiple.FailureKind);
        Assert.Equal(MessageReceiveFailureKind.Malformed, wrong.FailureKind);
    }

    [Fact]
    public void SignatureCoversExactUndecodedFormBody() {
        const string body = "command=%2Fstatus&user_id=U1&text=hello+world";
        var request = Request(body + " ");

        var result = SlackInteractionReceiver.Receive(
            request,
            SigningSecret,
            Sign(body),
            Timestamp);

        Assert.Equal(MessageReceiveFailureKind.Unauthorized, result.FailureKind);
        Assert.Equal(401, result.Acknowledgement.StatusCode);
    }

    [Fact]
    public void RequestDeduplicationKeyIsScopedToTrustedInstallationRoute() {
        const string body = "command=%2Fstatus&user_id=U1";
        var signature = Sign(body);
        var first = SlackInteractionReceiver.Receive(
            Request(body),
            SigningSecret,
            signature,
            Timestamp);
        var second = SlackInteractionReceiver.Receive(
            new MessageInboundRequest(
                "installation-2",
                "application/x-www-form-urlencoded",
                Encoding.UTF8.GetBytes(body),
                ReceivedAt),
            SigningSecret,
            signature,
            Timestamp);

        Assert.NotEqual(first.Envelope?.DeduplicationKey, second.Envelope?.DeduplicationKey);
    }

    private static MessageReceiveResult<SlackInteractionEvent> Receive(string body) =>
        SlackInteractionReceiver.Receive(
            Request(body),
            SigningSecret,
            Sign(body),
            Timestamp);

    private static MessageInboundRequest Request(string body) => new(
        "installation-1",
        "application/x-www-form-urlencoded; charset=utf-8",
        Encoding.UTF8.GetBytes(body),
        ReceivedAt) {
        CorrelationId = "slack-interaction-test"
    };

    private static string PayloadForm(string payload) => "payload=" + Uri.EscapeDataString(payload);

    private static string Sign(string body) {
        var requestBody = Encoding.UTF8.GetBytes(body);
        var prefix = Encoding.UTF8.GetBytes($"v0:{Timestamp}:");
        var signed = new byte[prefix.Length + requestBody.Length];
        Buffer.BlockCopy(prefix, 0, signed, 0, prefix.Length);
        Buffer.BlockCopy(requestBody, 0, signed, prefix.Length, requestBody.Length);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningSecret));
        return "v0=" + string.Concat(hmac.ComputeHash(signed).Select(value => value.ToString("x2")));
    }
}
