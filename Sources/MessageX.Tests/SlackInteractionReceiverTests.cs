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
        Assert.False(result.RequiresSynchronousDispatch);
        var persistedShape = JsonSerializer.Serialize(result.Envelope?.Payload);
        Assert.DoesNotContain("response_url", persistedShape, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("trigger-1", persistedShape, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", persistedShape, StringComparison.Ordinal);
    }

    [Fact]
    public void SignedSslCheckIsAcknowledgedWithoutCommandDispatch() {
        const string body = "ssl_check=1&token=legacy-verification-token";

        var result = Receive(body);

        Assert.Equal(MessageReceiveStatus.Acknowledged, result.Status);
        Assert.Equal(200, result.Acknowledgement.StatusCode);
        Assert.Null(result.Route);
        Assert.Null(result.Envelope);
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
              "message":{"thread_ts":"1787418500.000100"},
              "trigger_id":"trigger-2",
              "response_url":"https://hooks.slack.com/actions/T123/1/secret",
              "actions":[{"type":"button","action_id":"approve","value":"yes"}],
              "state":{"values":{"details":{"comment":{"type":"plain_text_input","value":"ready"}}}}
            }
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageRouteKind.Action, result.Route?.Kind);
        Assert.Equal("approve", result.Route?.Name);
        Assert.Equal(SlackInteractionKind.BlockAction, result.Envelope?.Payload.Kind);
        Assert.Equal("1787418599.000100", result.Envelope?.Message?.MessageId);
        Assert.Equal("1787418500.000100", result.Envelope?.Message?.ThreadId);
        Assert.Equal(MessageConversationKind.Thread, result.Envelope?.Conversation?.ConversationKind);
        Assert.Equal("button", result.Envelope?.Payload.ProviderPayload?.Actions[0].Type);
        Assert.Equal("yes", result.Envelope?.Payload.ProviderPayload?.Actions[0].Value);
        Assert.Equal("details", result.Envelope?.Payload.ProviderPayload?.State[0].BlockId);
        Assert.Equal("comment", result.Envelope?.Payload.ProviderPayload?.State[0].ActionId);
        Assert.Equal("ready", result.Envelope?.Payload.ProviderPayload?.State[0].Value);
        Assert.Equal("trigger-2", result.Envelope?.Payload.TransientContext.TriggerId);
        Assert.False(result.RequiresSynchronousDispatch);
    }

    [Theory]
    [InlineData("C999", "1787418599.000100")]
    [InlineData("C123", "1787418599.000200")]
    public void BlockActionRejectsConflictingDuplicateMessageCoordinates(
        string containerChannelId,
        string containerMessageTimestamp) {
        var payload = $$"""
            {"type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},"channel":{"id":"C123"},"container":{"channel_id":{{JsonSerializer.Serialize(containerChannelId)}},"message_ts":{{JsonSerializer.Serialize(containerMessageTimestamp)}}},"message":{"ts":"1787418599.000100"},"actions":[{"type":"button","action_id":"approve"}]}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageReceiveStatus.Rejected, result.Status);
        Assert.Equal(MessageReceiveFailureKind.Malformed, result.FailureKind);
    }

    [Fact]
    public void BlockActionStateFailsClosedWhenMalformedOrOversized() {
        const string malformed = """
            {"type":"block_actions","team":{"id":"T1"},"user":{"id":"U1"},"actions":[{"type":"button","action_id":"approve"}],"state":{"values":[]}}
            """;
        var inputs = Enumerable.Range(0, 257).ToDictionary(
            index => "input-" + index,
            _ => new { type = "plain_text_input", value = "ok" });
        var oversized = JsonSerializer.Serialize(new {
            type = "block_actions",
            team = new { id = "T1" },
            user = new { id = "U1" },
            actions = new[] { new { type = "button", action_id = "approve" } },
            state = new {
                values = new Dictionary<string, object> {
                    ["block"] = inputs
                }
            }
        });

        Assert.Equal(MessageReceiveStatus.Rejected, Receive(PayloadForm(malformed)).Status);
        Assert.Equal(MessageReceiveStatus.Rejected, Receive(PayloadForm(oversized)).Status);
    }

    [Theory]
    [InlineData(255, MessageReceiveStatus.DispatchReady)]
    [InlineData(256, MessageReceiveStatus.Rejected)]
    public void BlockActionEnforcesSlackActionIdBoundary(int length, MessageReceiveStatus expected) {
        var actionId = new string('a', length);
        var payload = $$"""
            {"type":"block_actions","team":{"id":"T1"},"user":{"id":"U1"},"actions":[{"type":"button","action_id":{{JsonSerializer.Serialize(actionId)}}}]}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(expected, result.Status);
    }

    [Theory]
    [InlineData(255, MessageReceiveStatus.DispatchReady)]
    [InlineData(256, MessageReceiveStatus.Rejected)]
    public void ViewStateEnforcesSlackActionIdBoundary(int length, MessageReceiveStatus expected) {
        var actionId = new string('a', length);
        var payload = """
            {"type":"view_submission","team":{"id":"T1"},"user":{"id":"U1"},"view":{"callback_id":"approval","state":{"values":{"block":{
            """ + JsonSerializer.Serialize(actionId) + """
            :{"type":"plain_text_input","value":"ok"}}}}}}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(expected, result.Status);
    }

    [Theory]
    [InlineData(255, MessageReceiveStatus.DispatchReady)]
    [InlineData(256, MessageReceiveStatus.Rejected)]
    public void ViewSubmissionEnforcesSlackCallbackIdBoundary(int length, MessageReceiveStatus expected) {
        var callbackId = new string('c', length);
        var payload = JsonSerializer.Serialize(new {
            type = "view_submission",
            team = new { id = "T1" },
            user = new { id = "U1" },
            view = new { callback_id = callbackId, state = new { values = new { } } }
        });

        Assert.Equal(expected, Receive(PayloadForm(payload)).Status);
    }

    [Theory]
    [InlineData(255, MessageReceiveStatus.DispatchReady)]
    [InlineData(256, MessageReceiveStatus.Rejected)]
    public void ViewStateEnforcesSlackBlockIdBoundary(int length, MessageReceiveStatus expected) {
        var blockId = new string('b', length);
        var payload = JsonSerializer.Serialize(new {
            type = "view_submission",
            team = new { id = "T1" },
            user = new { id = "U1" },
            view = new {
                callback_id = "approval",
                state = new {
                    values = new Dictionary<string, object> {
                        [blockId] = new { choice = new { type = "plain_text_input", value = "ok" } }
                    }
                }
            }
        });

        Assert.Equal(expected, Receive(PayloadForm(payload)).Status);
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
        Assert.True(result.RequiresSynchronousDispatch);
    }

    [Fact]
    public void ViewClosedDispatchesTheRequestedCallbackWithoutSubmissionSemantics() {
        const string payload = """
            {
              "type":"view_closed",
              "team":{"id":"T123"},
              "user":{"id":"U123"},
              "view":{"callback_id":"approval","private_metadata":"case-42"}
            }
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Equal(MessageRouteKind.Action, result.Route?.Kind);
        Assert.Equal(MessageEventKind.ActionInvoked, result.Envelope?.Kind);
        Assert.Equal(SlackInteractionKind.ViewClosed, result.Envelope?.Payload.Kind);
        Assert.Equal("case-42", result.Envelope?.Payload.ProviderPayload?.View?.PrivateMetadata);
        Assert.Empty(result.Envelope?.Payload.ProviderPayload?.View?.Values ?? Array.Empty<SlackViewStateInput>());
        Assert.False(result.RequiresSynchronousDispatch);
    }

    [Fact]
    public void ViewSubmissionPreservesPrivateMetadataAndFileInputIds() {
        const string payload = """
            {"type":"view_submission","team":{"id":"T123"},"enterprise":{"id":"E123"},"user":{"id":"U123"},"view":{"callback_id":"approval","private_metadata":"case-42","state":{"values":{"documents":{"evidence":{"type":"file_input","files":[{"id":"F1"},{"id":"F2"}]}}}}}}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal("T123", result.Envelope?.Payload.WorkspaceId);
        Assert.Equal("E123", result.Envelope?.Payload.EnterpriseId);
        Assert.Equal("case-42", result.Envelope?.Payload.ProviderPayload?.View?.PrivateMetadata);
        Assert.Equal(
            ["F1", "F2"],
            result.Envelope?.Payload.ProviderPayload?.View?.Values[0].FileIds ?? Array.Empty<string>());
    }

    [Fact]
    public void DateTimePickerSelectionIsPreservedAsItsUnixTimestamp() {
        const string payload = """
            {"type":"view_submission","team":{"id":"T123"},"user":{"id":"U123"},"view":{"callback_id":"schedule","state":{"values":{"when":{"start":{"type":"datetimepicker","selected_date_time":1787418600}}}}}}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal("1787418600", result.Envelope?.Payload.ProviderPayload?.View?.Values[0].SelectedValues[0]);
    }

    [Fact]
    public void RichTextInputIsPreservedWithoutNestedCapabilities() {
        const string payload = """
            {
              "type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},
              "actions":[{"type":"rich_text_input","action_id":"draft","rich_text_value":{"type":"rich_text","response_url":"https://secret.example","elements":[{"type":"rich_text_section","elements":[{"type":"text","text":"Hello"}]}]}}],
              "state":{"values":{"content":{"draft":{"type":"rich_text_input","rich_text_value":{"type":"rich_text","trigger_id":"secret-trigger","elements":[{"type":"rich_text_section","elements":[{"type":"text","text":"State"}]}]}}}}}
            }
            """;

        var result = Receive(PayloadForm(payload));

        var action = result.Envelope?.Payload.ProviderPayload?.Actions[0].RichTextValue;
        var state = result.Envelope?.Payload.ProviderPayload?.State[0].RichTextValue;
        Assert.Equal("Hello", action?.GetProperty("elements")[0].GetProperty("elements")[0]
            .GetProperty("text").GetString());
        Assert.Equal("State", state?.GetProperty("elements")[0].GetProperty("elements")[0]
            .GetProperty("text").GetString());
        Assert.False(action?.TryGetProperty("response_url", out _) ?? true);
        Assert.False(state?.TryGetProperty("trigger_id", out _) ?? true);
    }

    [Fact]
    public void RichTextInputRejectsAProjectionLargerThanFortyKilobytes() {
        var text = new string('a', 40001);
        var payload = $$$"""
            {"type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},"actions":[{"type":"rich_text_input","action_id":"draft","rich_text_value":{"type":"rich_text","elements":[{"type":"rich_text_section","elements":[{"type":"text","text":"{{{text}}}"}]}]}}]}
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal(MessageReceiveFailureKind.Malformed, result.FailureKind);
        Assert.Equal(400, result.Acknowledgement.StatusCode);
    }

    [Fact]
    public void MessageShortcutPreservesSelectedMessageCoordinates() {
        const string payload = """
            {
              "type":"message_action",
              "callback_id":"inspect",
              "team":{"id":"T123"},
              "user":{"id":"U123"},
              "trigger_id":"trigger-shortcut",
              "channel":{"id":"C123"},
              "message":{"ts":"1787418599.000200","text":"selected"}
            }
            """;

        var result = Receive(PayloadForm(payload));

        Assert.Equal("1787418599.000200", result.Envelope?.Message?.MessageId);
        Assert.Equal("selected", result.Envelope?.Payload.ProviderPayload?.Message?.Text);
        Assert.False(result.RequiresSynchronousDispatch);
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

    [Fact]
    public void VerifiedWorkspaceResolverScopesTheInteractionToTheResolvedInstallation() {
        const string body = "command=%2Fstatus&api_app_id=A1&team_id=T2&user_id=U1&channel_id=C1";

        var result = SlackInteractionReceiver.Receive(
            Request(body),
            SigningSecret,
            Sign(body),
            Timestamp,
            installationResolver: context => context.WorkspaceId == "T2" ? "workspace-two" : null);
        var otherInstallation = SlackInteractionReceiver.Receive(
            Request(body),
            SigningSecret,
            Sign(body),
            Timestamp,
            installationResolver: _ => "workspace-other");

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Equal("workspace-two", result.Envelope?.InstallationId);
        Assert.Equal("workspace-two", result.Envelope?.Conversation?.InstallationId);
        Assert.NotEqual(
            result.Envelope?.DeduplicationKey,
            otherInstallation.Envelope?.DeduplicationKey);
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
