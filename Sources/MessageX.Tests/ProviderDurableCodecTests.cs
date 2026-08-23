using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
              "state":{"values":{"one":{"two":{"value":"handler-value","response_url":"nested-url"}}}}
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
    public void DiscordCodecRetainsOptionsButDropsInteractionAndNestedTokens()
    {
        const string json = """
            {
              "id":"100000000000000001","application_id":"100000000000000002","type":2,
              "token":"interaction-secret","guild_id":"100000000000000003","channel_id":"100000000000000004",
              "member":{"user":{"id":"100000000000000005"}},
              "data":{"name":"status","type":1,"options":[{"name":"target","value":"server-1","token":"nested-secret"}]}
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
        Assert.Equal("server-1", decoded.Payload.Data.GetProperty("options")[0].GetProperty("value").GetString());
        Assert.False(decoded.Payload.TransientContext.CanFollowUp);
        Assert.Null(decoded.Payload.TransientContext.Token);
        Assert.Equal("100000000000000004", decoded.Conversation?.ConversationId);
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

        Assert.Equal("approve", discordDecoded.Payload.Name);
        Assert.Equal(" approve ", discordDecoded.Payload.Data.GetProperty("custom_id").GetString());
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
            ConversationId = "C123"
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
