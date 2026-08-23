using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MessageX.Discord;
using MessageX.Discord.Hosting.AspNetCore;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Slack;
using MessageX.Slack.Hosting.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace MessageX.Tests;

public sealed class ProviderDurableCodecTests
{
    private const string SlackSecret = "durable-signing-secret";
    private const string SlackTimestamp = "1787418600";
    private const string DiscordTimestamp = "1787420400";
    private static readonly DateTimeOffset ReceivedAt = DateTimeOffset.FromUnixTimeSeconds(1787418600);
    private static readonly Ed25519PrivateKeyParameters DiscordPrivateKey = new(
        Enumerable.Range(1, 32).Select(value => (byte)value).ToArray(),
        0);

    [Fact]
    public void SlackInteractionCodecRetainsActionDataButRemovesCapabilitiesRecursively()
    {
        const string json = """
            {
              "type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},
              "channel":{"id":"C123"},"trigger_id":"trigger-secret",
              "response_url":"https://hooks.slack.com/actions/secret",
              "actions":[{"type":"button","action_id":"approve","value":"yes","token":"nested-secret"}],
              "state":{"values":{"one":{"two":{"type":"plain_text_input","value":"handler-value","response_url":"nested-url"}}}}
            }
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();

        var record = codec.Encode(receive.Route!, receive.Envelope!);
        var stored = Encoding.UTF8.GetString(record.CopyPayload());
        var decoded = codec.Decode(record);

        Assert.DoesNotContain("trigger-secret", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("hooks.slack.com", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("nested-secret", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("nested-url", stored, StringComparison.Ordinal);
        Assert.Equal("yes", decoded.Payload.ProviderPayload?.Actions[0].Value);
        Assert.Equal("one", decoded.Payload.ProviderPayload?.State[0].BlockId);
        Assert.Equal("two", decoded.Payload.ProviderPayload?.State[0].ActionId);
        Assert.Equal("plain_text_input", decoded.Payload.ProviderPayload?.State[0].Type);
        Assert.Equal("handler-value", decoded.Payload.ProviderPayload?.State[0].Value);
        Assert.Empty(decoded.Payload.ProviderPayload?.View?.Values ?? Array.Empty<SlackViewStateInput>());
        Assert.Null(decoded.Payload.TransientContext.TriggerId);
        Assert.Null(decoded.Payload.TransientContext.ResponseUrl);
        Assert.Equal(receive.Envelope?.Conversation?.ConversationId, decoded.Conversation?.ConversationId);
    }

    [Fact]
    public void SlackEventCodecRetainsEventDataButRemovesNestedAuthorization()
    {
        const string json = """
            {
              "type":"event_callback","team_id":"T123","event_id":"Ev123","event_time":1787418599,
              "event":{"type":"app_mention","user":"U123","channel":"C123","ts":"1787418599.1",
                "text":"hello","files":[{"name":"report.txt","token":"file-secret"}],
                "authorization":{"enterprise_id":"E123"}}
            }
            """;
        var receive = SlackEventsApiReceiver.Receive(
            Request("application/json", json),
            SlackSecret,
            SlackSign(json),
            SlackTimestamp);
        var codec = new SlackInboundEventDurableCodec();

        var record = codec.Encode(receive.Route!, receive.Envelope!);
        var stored = Encoding.UTF8.GetString(record.CopyPayload());
        var decoded = codec.Decode(record);

        Assert.DoesNotContain("file-secret", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("authorization", stored, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("report.txt", stored, StringComparison.Ordinal);
        Assert.True(record.PayloadLength < 100000);
        Assert.Equal("hello", decoded.Payload.ProviderEvent.Text);
        Assert.Equal("Ev123", decoded.EventId);
        Assert.Equal(MessageConversationKind.Channel, decoded.Conversation?.ConversationKind);
    }

    [Fact]
    public void SlackModalCodecRetainsIdentityMetadataAndFileInputs()
    {
        const string json = """
            {"type":"view_submission","team":{"id":"T123"},"enterprise":{"id":"E123"},"user":{"id":"U123"},"view":{"callback_id":"approval","private_metadata":"case-42","state":{"values":{"documents":{"evidence":{"type":"file_input","files":[{"id":"F1"},{"id":"F2"}]}}}}}}
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();

        var decoded = codec.Decode(codec.Encode(receive.Route!, receive.Envelope!));

        Assert.Equal("T123", decoded.Payload.WorkspaceId);
        Assert.Equal("E123", decoded.Payload.EnterpriseId);
        Assert.Equal("case-42", decoded.Payload.ProviderPayload?.View?.PrivateMetadata);
        Assert.Equal(
            ["F1", "F2"],
            decoded.Payload.ProviderPayload?.View?.Values[0].FileIds ?? Array.Empty<string>());
        Assert.Equal("T123", decoded.ScopeId);
    }

    [Fact]
    public void SlackRichTextInputSurvivesDurableRoundTripWithoutCapabilities()
    {
        const string json = """
            {"type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},"actions":[{"type":"rich_text_input","action_id":"draft","rich_text_value":{"type":"rich_text","response_url":"https://secret.example","elements":[{"type":"rich_text_section","elements":[{"type":"text","text":"Hello durable world"}]}]}}]}
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();

        var record = codec.Encode(receive.Route!, receive.Envelope!);
        var stored = Encoding.UTF8.GetString(record.CopyPayload());
        var decoded = codec.Decode(record);
        var richText = decoded.Payload.ProviderPayload?.Actions[0].RichTextValue;

        Assert.Equal("Hello durable world", richText?.GetProperty("elements")[0]
            .GetProperty("elements")[0].GetProperty("text").GetString());
        Assert.DoesNotContain("secret.example", stored, StringComparison.Ordinal);
        Assert.False(richText?.TryGetProperty("response_url", out _) ?? true);
    }

    [Fact]
    public void DiscordCodecRetainsOptionsButDropsInteractionAndNestedTokens()
    {
        const string json = """
            {
              "id":"100000000000000001","application_id":"100000000000000002","type":2,
              "token":"interaction-secret","guild_id":"100000000000000003","channel_id":"100000000000000004",
              "member":{"user":{"id":"100000000000000005"}},
              "data":{"name":"status","type":1,"options":[{"name":"target","value":"server-1","token":"nested-secret"}],"resolved":{"attachments":{"100000000000000006":{"id":"100000000000000006","filename":"proof.txt","url":"https://cdn.discord.example/signed","proxy_url":"https://proxy.discord.example/signed"}}}}
            }
            """;
        var signature = DiscordSign(DiscordTimestamp, json);
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            signature,
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();

        var record = codec.Encode(receive.Route!, receive.Envelope!);
        var stored = Encoding.UTF8.GetString(record.CopyPayload());
        var decoded = codec.Decode(record);

        Assert.DoesNotContain("interaction-secret", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("nested-secret", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("cdn.discord.example", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("proxy.discord.example", stored, StringComparison.Ordinal);
        Assert.Equal("server-1", decoded.Payload.Data.GetProperty("options")[0].GetProperty("value").GetString());
        var attachment = decoded.Payload.Data.GetProperty("resolved").GetProperty("attachments")
            .GetProperty("100000000000000006");
        Assert.Equal("proof.txt", attachment.GetProperty("filename").GetString());
        Assert.False(attachment.TryGetProperty("url", out _));
        Assert.False(attachment.TryGetProperty("proxy_url", out _));
        Assert.False(decoded.Payload.TransientContext.CanFollowUp);
        Assert.Null(decoded.Payload.TransientContext.Token);
        Assert.Equal("100000000000000002", decoded.Payload.ApplicationId);
        Assert.Equal("100000000000000004", decoded.Conversation?.ConversationId);
    }

    [Fact]
    public void DiscordCodecUsesPersistedApplicationIdentityAndRejectsMissingIdentity()
    {
        const string json = """
            {"id":"100000000000000001","application_id":"100000000000000002","type":2,
             "token":"interaction-secret","guild_id":"100000000000000003",
             "user":{"id":"100000000000000004"},"data":{"name":"status","type":1}}
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var receivedEnvelope = Assert.IsType<MessageEventEnvelope<DiscordInboundInteraction>>(receive.Envelope);
        var payload = receivedEnvelope.Payload;
        var rehydrated = JsonSerializer.Deserialize<DiscordInboundInteraction>(
            JsonSerializer.Serialize(payload));
        var envelope = new MessageEventEnvelope<DiscordInboundInteraction>(
            MessageProviders.Discord,
            receivedEnvelope.InstallationId,
            receivedEnvelope.DeduplicationKey,
            MessageEventKind.CommandInvoked,
            ReceivedAt,
            Assert.IsType<DiscordInboundInteraction>(rehydrated)) {
            EventId = receivedEnvelope.EventId,
            ScopeId = receivedEnvelope.ScopeId,
            SenderId = receivedEnvelope.SenderId
        };
        var codec = new DiscordInteractionDurableCodec();

        var decoded = codec.Decode(codec.Encode(MessageRoute.ForCommand("status", "1"), envelope));

        Assert.Equal("100000000000000002", decoded.Payload.ApplicationId);
        Assert.Equal("100000000000000002", decoded.Payload.TransientContext.ApplicationId);

        var mismatchedIdentityEnvelope = new MessageEventEnvelope<DiscordInboundInteraction>(
            MessageProviders.Discord,
            receivedEnvelope.InstallationId,
            "discord-request:" + new string('0', 64),
            MessageEventKind.CommandInvoked,
            ReceivedAt,
            Assert.IsType<DiscordInboundInteraction>(rehydrated)) {
            EventId = receivedEnvelope.EventId,
            ScopeId = receivedEnvelope.ScopeId,
            SenderId = receivedEnvelope.SenderId
        };
        Assert.Throws<MessageDurablePayloadException>(() =>
            codec.Encode(MessageRoute.ForCommand("status", "1"), mismatchedIdentityEnvelope));

        var missing = new DiscordInboundInteraction(
            DiscordInteractionKind.ApplicationCommand,
            "status",
            null,
            null,
            null,
            0,
            DiscordApplicationCommandType.ChatInput,
            null,
            payload.Data);
        var missingEnvelope = new MessageEventEnvelope<DiscordInboundInteraction>(
            MessageProviders.Discord,
            receivedEnvelope.InstallationId,
            receivedEnvelope.DeduplicationKey,
            MessageEventKind.CommandInvoked,
            ReceivedAt,
            missing) {
            EventId = receivedEnvelope.EventId,
            ScopeId = receivedEnvelope.ScopeId,
            SenderId = receivedEnvelope.SenderId
        };
        Assert.Throws<MessageDurablePayloadException>(() =>
            codec.Encode(MessageRoute.ForCommand("status", "1"), missingEnvelope));
    }

    [Fact]
    public void DiscordCodecRejectsReducedPublicProjectionBeforePersistence()
    {
        var payload = new DiscordInboundInteraction(
            DiscordInteractionKind.ApplicationCommand,
            "status",
            null,
            "en-US",
            null,
            0,
            DiscordApplicationCommandType.ChatInput,
            null,
            "100000000000000002");
        var envelope = new MessageEventEnvelope<DiscordInboundInteraction>(
            MessageProviders.Discord,
            "installation-a",
            "discord-reduced-projection",
            MessageEventKind.CommandInvoked,
            ReceivedAt,
            payload) {
            EventId = "100000000000000001",
            ScopeId = "100000000000000003",
            SenderId = "100000000000000004"
        };

        Assert.Throws<MessageDurablePayloadException>(() =>
            new DiscordInteractionDurableCodec().Encode(MessageRoute.ForCommand("status", "1"), envelope));
    }

    [Fact]
    public void ProviderCodecsRoundTripNormalizedRouteIdentifiers()
    {
        const string slackJson = """
            {
              "type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},
              "actions":[{"type":"button","action_id":" approve ","value":"yes"}]
            }
            """;
        var slackBody = "payload=" + Uri.EscapeDataString(slackJson);
        var slackReceive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", slackBody),
            SlackSecret,
            SlackSign(slackBody),
            SlackTimestamp);
        var slackCodec = new SlackInteractionEventDurableCodec();

        var slackDecoded = slackCodec.Decode(slackCodec.Encode(slackReceive.Route!, slackReceive.Envelope!));

        Assert.Equal("approve", slackDecoded.Payload.Name);
        Assert.Equal("approve", slackDecoded.Payload.ProviderPayload?.Actions[0].ActionId);

        const string discordJson = """
            {
              "id":"100000000000000001","application_id":"100000000000000002","type":3,
              "token":"interaction-secret","guild_id":"100000000000000003","channel_id":"100000000000000004",
              "member":{"user":{"id":"100000000000000005"}},
              "data":{"custom_id":" approve "}
            }
            """;
        var discordReceive = DiscordInteractionReceiver.Receive(
            Request("application/json", discordJson, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, discordJson),
            DiscordTimestamp);
        var discordCodec = new DiscordInteractionDurableCodec();

        var discordDecoded = discordCodec.Decode(discordCodec.Encode(discordReceive.Route!, discordReceive.Envelope!));

        Assert.Equal(" approve ", discordDecoded.Payload.Name);
        Assert.Equal(" approve ", discordDecoded.Payload.Data.GetProperty("custom_id").GetString());
    }

    [Fact]
    public void DiscordContextCommandCodecRetainsSafeTargetCoordinate()
    {
        const string json = """
            {
              "id":"100000000000000101","application_id":"100000000000000102","type":2,
              "token":"interaction-secret","channel_id":"100000000000000103",
              "user":{"id":"100000000000000104"},
              "data":{"name":"inspect","type":3,"target_id":"100000000000000105"}
            }
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();

        var decoded = codec.Decode(codec.Encode(receive.Route!, receive.Envelope!));

        Assert.Equal("100000000000000105", decoded.Payload.TargetId);
        Assert.Equal("100000000000000105", decoded.Message?.MessageId);
    }

    [Fact]
    public void SlackReactionCodecRequiresActorAndMessageTargetCoordinates()
    {
        const string json = """
            {"type":"event_callback","team_id":"T1","event_id":"EvReaction","event":{"type":"reaction_added","user":"U3","reaction":"thumbsup","event_ts":"1787416801.1","item":{"type":"message","channel":"C2","ts":"1787416799.3"}}}
            """;
        var receive = SlackEventsApiReceiver.Receive(
            Request("application/json", json),
            SlackSecret,
            SlackSign(json),
            SlackTimestamp);
        var codec = new SlackInboundEventDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var changed = Encoding.UTF8.GetString(valid.CopyPayload()).Replace(
            "\"reaction\":\"thumbsup\"",
            "\"reaction\":null",
            StringComparison.Ordinal);
        var tampered = new MessageDurableRecord(valid.Provider, valid.InstallationId,
            valid.DeduplicationKey, valid.Route, valid.ReceivedAt, valid.PayloadType,
            Encoding.UTF8.GetBytes(changed));

        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
    }

    [Fact]
    public void DiscordFollowUpCapabilityExpires()
    {
        var active = new DiscordTransientInteractionContext(
            "100000000000000002",
            "token",
            DateTimeOffset.UtcNow.AddMinutes(1));
        var expired = new DiscordTransientInteractionContext(
            "100000000000000002",
            "token",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(active.CanFollowUp);
        Assert.False(expired.CanFollowUp);
    }

    [Fact]
    public void SlackCodecRejectsMalformedBlockActionAsDurablePayloadFailure()
    {
        const string json = """
            {
              "type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},
              "actions":[{"type":"button","action_id":"approve","value":"yes"}]
            }
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var changed = Encoding.UTF8.GetString(valid.CopyPayload()).Replace(
            "\"actions\":[{",
            "\"actions\":[null,{",
            StringComparison.Ordinal);
        var tampered = new MessageDurableRecord(
            valid.Provider,
            valid.InstallationId,
            valid.DeduplicationKey,
            valid.Route,
            valid.ReceivedAt,
            valid.PayloadType,
            Encoding.UTF8.GetBytes(changed));

        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
    }

    [Fact]
    public void SlackInteractionCodecRejectsSenderMetadataThatConflictsWithVerifiedPayload()
    {
        const string body = "command=%2Fstatus&user_id=U123&team_id=T123&channel_id=C123";
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var projection = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        projection["metadata"]!["senderId"] = "U999";
        var tampered = CopyWithPayload(valid, Encoding.UTF8.GetBytes(projection.ToJsonString()));

        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
        receive.Envelope!.SenderId = "U999";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Encode(receive.Route!, receive.Envelope));
    }

    [Fact]
    public void SlackInteractionCodecBindsVerifiedConversationAndMessageCoordinates()
    {
        const string json = """
            {"type":"message_action","callback_id":"inspect","team":{"id":"T123"},"user":{"id":"U123"},"channel":{"id":"C123"},"message":{"ts":"1787418599.000200","text":"selected"}}
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);

        var changedConversation = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        changedConversation["metadata"]!["conversation"]!["conversationId"] = "C999";
        changedConversation["metadata"]!["message"]!["conversationId"] = "C999";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(
            CopyWithPayload(valid, Encoding.UTF8.GetBytes(changedConversation.ToJsonString()))));

        var changedMessage = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        changedMessage["metadata"]!["message"]!["messageId"] = "1787418599.000201";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(
            CopyWithPayload(valid, Encoding.UTF8.GetBytes(changedMessage.ToJsonString()))));
    }

    [Fact]
    public void SlackMessageShortcutCodecRejectsConflictingSelectedMessageTimestamp()
    {
        const string json = """
            {"type":"message_action","callback_id":"inspect","team":{"id":"T123"},"user":{"id":"U123"},"channel":{"id":"C123"},"message":{"ts":"1787418599.000200","text":"selected"}}
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var changed = Encoding.UTF8.GetString(valid.CopyPayload()).Replace(
            "1787418599.000200",
            "1787418599.000201",
            StringComparison.Ordinal);
        var tampered = new MessageDurableRecord(
            valid.Provider,
            valid.InstallationId,
            valid.DeduplicationKey,
            valid.Route,
            valid.ReceivedAt,
            valid.PayloadType,
            Encoding.UTF8.GetBytes(changed));

        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
    }

    [Fact]
    public void ProviderCodecConstructorFailuresAndExactRoutesArePermanentPayloadErrors()
    {
        const string json = """
            {
              "type":"block_actions","team":{"id":"T123"},"user":{"id":"U123"},
              "actions":[{"type":"button","action_id":"approve","value":"yes"}]
            }
            """;
        var body = "payload=" + Uri.EscapeDataString(json);
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var slackCodec = new SlackInteractionEventDurableCodec();
        var valid = slackCodec.Encode(receive.Route!, receive.Envelope!);
        var malformedJson = Encoding.UTF8.GetString(valid.CopyPayload()).Replace(
            "\"actionId\":\"approve\"",
            "\"actionId\":null",
            StringComparison.Ordinal);
        var malformed = new MessageDurableRecord(
            valid.Provider,
            valid.InstallationId,
            valid.DeduplicationKey,
            valid.Route,
            valid.ReceivedAt,
            valid.PayloadType,
            Encoding.UTF8.GetBytes(malformedJson));
        var changedRoute = new MessageDurableRecord(
            valid.Provider,
            valid.InstallationId,
            valid.DeduplicationKey,
            MessageRoute.ForAction("APPROVE"),
            valid.ReceivedAt,
            valid.PayloadType,
            valid.CopyPayload());

        Assert.Throws<MessageDurablePayloadException>(() => slackCodec.Decode(malformed));
        Assert.Throws<MessageDurablePayloadException>(() => slackCodec.Decode(changedRoute));
    }

    [Fact]
    public void DiscordCodecBoundsTheCompleteProjection()
    {
        var largeValue = new string('a', 1_048_200);
        var json = $$"""
            {
              "id":"100000000000000001","application_id":"100000000000000002","type":2,
              "token":"interaction-secret","channel_id":"100000000000000004",
              "user":{"id":"100000000000000005"},
              "data":{"name":"status","type":1,"options":[{"name":"target","value":"{{largeValue}}"}]}
            }
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();

        Assert.Equal(MessageReceiveStatus.DispatchReady, receive.Status);
        Assert.Throws<MessageDurablePayloadException>(() => codec.Encode(receive.Route!, receive.Envelope!));
    }

    [Fact]
    public void DiscordCodecRejectsTamperedCommandTypeAndInstallationOwner()
    {
        const string json = """
            {
              "id":"100000000000000001","application_id":"100000000000000002","type":2,
              "token":"interaction-secret","channel_id":"100000000000000004",
              "user":{"id":"100000000000000005"},
              "authorizing_integration_owners":{"1":"100000000000000006"},
              "data":{"name":"status","type":1}
            }
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var stored = Encoding.UTF8.GetString(valid.CopyPayload());

        foreach (var changed in new[] {
                     stored.Replace("\"type\":1", "\"type\":2", StringComparison.Ordinal),
                     stored.Replace("100000000000000006", "tenant-a", StringComparison.Ordinal),
                     stored.Replace("100000000000000006", "0", StringComparison.Ordinal)
                 }) {
            var tampered = new MessageDurableRecord(valid.Provider, valid.InstallationId,
                valid.DeduplicationKey, valid.Route, valid.ReceivedAt, valid.PayloadType,
                Encoding.UTF8.GetBytes(changed));
            Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
        }
    }

    [Fact]
    public void DiscordCodecRejectsTamperedContextTargetAndMetadataSnowflakes()
    {
        const string json = """
            {"id":"100000000000000101","application_id":"100000000000000102","type":2,"token":"interaction-secret","channel_id":"100000000000000103","user":{"id":"100000000000000104"},"data":{"name":"inspect","type":3,"target_id":"100000000000000105"}}
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var stored = Encoding.UTF8.GetString(valid.CopyPayload());

        foreach (var changed in new[] {
                     stored.Replace("\"target_id\":\"100000000000000105\"", "\"target_id\":\"100000000000000106\"", StringComparison.Ordinal),
                     stored.Replace("\"eventId\":\"100000000000000101\"", "\"eventId\":\"100000000000000106\"", StringComparison.Ordinal),
                     stored.Replace("\"eventId\":\"100000000000000101\"", "\"eventId\":\"not-a-snowflake\"", StringComparison.Ordinal)
                 }) {
            var tampered = new MessageDurableRecord(
                valid.Provider,
                valid.InstallationId,
                valid.DeduplicationKey,
                valid.Route,
                valid.ReceivedAt,
                valid.PayloadType,
                Encoding.UTF8.GetBytes(changed));
            Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
        }
    }

    [Fact]
    public void DiscordCodecRejectsSenderMetadataThatConflictsWithVerifiedPayload()
    {
        const string json = """
            {"id":"100000000000000101","application_id":"100000000000000102","type":2,"token":"interaction-secret","channel_id":"100000000000000103","user":{"id":"100000000000000104"},"data":{"name":"status","type":1}}
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);
        var projection = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        projection["metadata"]!["senderId"] = "100000000000000199";
        var tampered = CopyWithPayload(valid, Encoding.UTF8.GetBytes(projection.ToJsonString()));

        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
        receive.Envelope!.SenderId = "100000000000000199";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Encode(receive.Route!, receive.Envelope));
    }

    [Fact]
    public void DiscordCodecBindsVerifiedScopeConversationAndMessageCoordinates()
    {
        const string json = """
            {"id":"100000000000000101","application_id":"100000000000000102","type":2,"token":"interaction-secret","guild_id":"100000000000000103","channel_id":"100000000000000104","member":{"user":{"id":"100000000000000105"}},"data":{"name":"inspect","type":3,"target_id":"100000000000000106"}}
            """;
        var receive = DiscordInteractionReceiver.Receive(
            Request("application/json", json, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, json),
            DiscordTimestamp);
        var codec = new DiscordInteractionDurableCodec();
        var valid = codec.Encode(receive.Route!, receive.Envelope!);

        var changedReferences = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        changedReferences["metadata"]!["conversation"]!["conversationId"] = "100000000000000199";
        changedReferences["metadata"]!["message"]!["conversationId"] = "100000000000000199";
        changedReferences["metadata"]!["message"]!["messageId"] = "100000000000000198";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(
            CopyWithPayload(valid, Encoding.UTF8.GetBytes(changedReferences.ToJsonString()))));

        var changedScope = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        changedScope["metadata"]!["scopeId"] = "100000000000000197";
        changedScope["metadata"]!["conversation"]!["scopeId"] = "100000000000000197";
        changedScope["metadata"]!["message"]!["scopeId"] = "100000000000000197";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(
            CopyWithPayload(valid, Encoding.UTF8.GetBytes(changedScope.ToJsonString()))));

        var changedProviderMessage = JsonNode.Parse(valid.CopyPayload())!.AsObject();
        changedProviderMessage["messageId"] = "100000000000000196";
        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(
            CopyWithPayload(valid, Encoding.UTF8.GetBytes(changedProviderMessage.ToJsonString()))));
    }

    [Fact]
    public void ProviderEndpointRegistrationIncludesOnlyItsOwnDurableCodecs()
    {
        var slack = new ServiceCollection().AddMessageXSlackAspNetCore().BuildServiceProvider();
        var discord = new ServiceCollection().AddMessageXDiscordAspNetCore().BuildServiceProvider();

        Assert.NotNull(slack.GetService<IMessageDurableCodec<SlackInboundEvent>>());
        Assert.NotNull(slack.GetService<IMessageDurableCodec<SlackInteractionEvent>>());
        Assert.Null(slack.GetService<IMessageDurableCodec<DiscordInboundInteraction>>());
        Assert.NotNull(discord.GetService<IMessageDurableCodec<DiscordInboundInteraction>>());
        Assert.Null(discord.GetService<IMessageDurableCodec<SlackInboundEvent>>());
    }

    [Fact]
    public void ProviderCodecsRejectForeignEnvelopesBeforePersistence()
    {
        const string slackEventJson = """
            {"type":"event_callback","team_id":"T123","event_id":"Ev123","event_time":1787418599,"event":{"type":"app_mention","user":"U123","channel":"C123","ts":"1787418599.1","text":"hello"}}
            """;
        var slackEvent = SlackEventsApiReceiver.Receive(
            Request("application/json", slackEventJson),
            SlackSecret,
            SlackSign(slackEventJson),
            SlackTimestamp);
        const string slackBody = "command=%2Fstatus&user_id=U123&team_id=T123";
        var slackInteraction = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", slackBody),
            SlackSecret,
            SlackSign(slackBody),
            SlackTimestamp);
        const string discordJson = """
            {"id":"100000000000000101","application_id":"100000000000000102","type":2,"token":"interaction-secret","user":{"id":"100000000000000104"},"data":{"name":"status","type":1}}
            """;
        var discord = DiscordInteractionReceiver.Receive(
            Request("application/json", discordJson, DateTimeOffset.FromUnixTimeSeconds(1787420400)),
            Convert.ToHexString(DiscordPrivateKey.GeneratePublicKey().GetEncoded()),
            DiscordSign(DiscordTimestamp, discordJson),
            DiscordTimestamp);

        Assert.Throws<MessageDurablePayloadException>(() =>
            new SlackInboundEventDurableCodec().Encode(
                slackEvent.Route!,
                CopyWithProvider(slackEvent.Envelope!, MessageProviders.Discord)));
        Assert.Throws<MessageDurablePayloadException>(() =>
            new SlackInteractionEventDurableCodec().Encode(
                slackInteraction.Route!,
                CopyWithProvider(slackInteraction.Envelope!, MessageProviders.Discord)));
        Assert.Throws<MessageDurablePayloadException>(() =>
            new DiscordInteractionDurableCodec().Encode(
                discord.Route!,
                CopyWithProvider(discord.Envelope!, MessageProviders.Slack)));
    }

    [Fact]
    public void CodecRejectsUnsafeReferenceBeforeStorageAndRouteTamperingAfterRestart()
    {
        const string body = "command=%2Fstatus&user_id=U123&team_id=T123&channel_id=C123";
        var receive = SlackInteractionReceiver.Receive(
            Request("application/x-www-form-urlencoded", body),
            SlackSecret,
            SlackSign(body),
            SlackTimestamp);
        var codec = new SlackInteractionEventDurableCodec();
        receive.Envelope!.Conversation = new MessageReference(MessageProviders.Discord)
        {
            InstallationId = receive.Envelope.InstallationId,
            ConversationId = "C123"
        };

        Assert.Throws<ArgumentException>(() => codec.Encode(receive.Route!, receive.Envelope));

        receive.Envelope.Conversation = new MessageReference(MessageProviders.Slack)
        {
            InstallationId = receive.Envelope.InstallationId,
            ScopeId = "other-scope",
            ConversationId = "C123"
        };
        Assert.Throws<ArgumentException>(() => codec.Encode(receive.Route!, receive.Envelope));

        receive.Envelope.Conversation = new MessageReference(MessageProviders.Slack)
        {
            InstallationId = receive.Envelope.InstallationId,
            ScopeId = receive.Envelope.ScopeId,
            ConversationId = "C123",
            ConversationKind = MessageConversationKind.Channel
        };
        var valid = codec.Encode(receive.Route!, receive.Envelope);
        var changed = Encoding.UTF8.GetString(valid.CopyPayload()).Replace(
            "\"name\":\"status\"",
            "\"name\":\"other\"",
            StringComparison.Ordinal);
        var tampered = new MessageDurableRecord(
            valid.Provider,
            valid.InstallationId,
            valid.DeduplicationKey,
            valid.Route,
            valid.ReceivedAt,
            valid.PayloadType,
            Encoding.UTF8.GetBytes(changed));

        Assert.Throws<MessageDurablePayloadException>(() => codec.Decode(tampered));
    }

    private static MessageInboundRequest Request(
        string contentType,
        string body,
        DateTimeOffset? receivedAt = null) => new(
        "installation-1",
        contentType,
        Encoding.UTF8.GetBytes(body),
        receivedAt ?? ReceivedAt);

    private static MessageDurableRecord CopyWithPayload(MessageDurableRecord record, byte[] payload) => new(
        record.Provider,
        record.InstallationId,
        record.DeduplicationKey,
        record.Route,
        record.ReceivedAt,
        record.PayloadType,
        payload);

    private static MessageEventEnvelope<TProviderPayload> CopyWithProvider<TProviderPayload>(
        MessageEventEnvelope<TProviderPayload> envelope,
        string provider) => new(
            provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            envelope.Kind,
            envelope.ReceivedAt,
            envelope.Payload) {
                EventId = envelope.EventId,
                ScopeId = envelope.ScopeId,
                SenderId = envelope.SenderId,
                Conversation = envelope.Conversation,
                Message = envelope.Message,
                EventTime = envelope.EventTime,
                CorrelationId = envelope.CorrelationId
            };

    private static string SlackSign(string body)
    {
        var prefix = Encoding.UTF8.GetBytes($"v0:{SlackTimestamp}:");
        var bytes = Encoding.UTF8.GetBytes(body);
        var signed = new byte[prefix.Length + bytes.Length];
        Buffer.BlockCopy(prefix, 0, signed, 0, prefix.Length);
        Buffer.BlockCopy(bytes, 0, signed, prefix.Length, bytes.Length);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SlackSecret));
        return "v0=" + string.Concat(hmac.ComputeHash(signed).Select(value => value.ToString("x2")));
    }

    private static string DiscordSign(string timestamp, string json)
    {
        var signer = new Ed25519Signer();
        signer.Init(true, DiscordPrivateKey);
        var timestampBytes = Encoding.ASCII.GetBytes(timestamp);
        var body = Encoding.UTF8.GetBytes(json);
        signer.BlockUpdate(timestampBytes, 0, timestampBytes.Length);
        signer.BlockUpdate(body, 0, body.Length);
        return Convert.ToHexString(signer.GenerateSignature());
    }
}
