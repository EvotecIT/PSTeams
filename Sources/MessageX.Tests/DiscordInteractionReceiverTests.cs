using System.Text;
using System.Text.Json;
using MessageX.Discord;
using MessageX.Hosting;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace MessageX.Tests;

public sealed class DiscordInteractionReceiverTests {
    private const string Timestamp = "1787420400";
    private static readonly DateTimeOffset ReceivedAt = DateTimeOffset.FromUnixTimeSeconds(1787420400);
    private static readonly Ed25519PrivateKeyParameters PrivateKey = new(
        Enumerable.Range(1, 32).Select(value => (byte)value).ToArray(),
        0);
    private static readonly string PublicKeyHex = Convert.ToHexString(
        PrivateKey.GeneratePublicKey().GetEncoded());

    [Fact]
    public void FixedIndependentPingVectorReturnsPongWithoutDispatch() {
        const string publicKey = "79B5562E8FE654F94078B112E8A98BA7901F853AE695BED7E0E3910BAD049664";
        const string signature = "7FB1DF4757DE1D90C78D7AC9CAA2FD7B00B003DAFB9E49C24C49833E71559EC9" +
                                 "5694335976D724D0A98DC692B400D271AAF043036F35540523D6CF35F2D56F07";
        const string timestamp = "1787301000";
        const string json = "{\"type\":1}";
        var request = Request(json, DateTimeOffset.FromUnixTimeSeconds(1787301000));

        var result = DiscordInteractionReceiver.Receive(
            request,
            publicKey,
            signature,
            timestamp);

        Assert.Equal(MessageReceiveStatus.Acknowledged, result.Status);
        Assert.Null(result.Envelope);
        Assert.Equal(1, AckType(result.Acknowledgement));
    }

    [Fact]
    public void CommandProducesDeferredTypedEnvelopeAndRedactsTransientToken() {
        var interactionCreatedAt = ReceivedAt.AddMinutes(-1);
        var interactionId = SnowflakeAt(interactionCreatedAt);
        var json = $$$"""
            {
              "id":"{{{interactionId}}}",
              "application_id":"100000000000000002",
              "type":2,
              "token":"interaction-secret-token",
              "guild_id":"100000000000000003",
              "channel_id":"100000000000000004",
              "member":{"user":{"id":"100000000000000005"}},
              "authorizing_integration_owners":{"0":"100000000000000003"},
              "locale":"en-US",
              "guild_locale":"en-GB",
              "context":0,
              "data":{"id":"100000000000000006","name":"status","type":1}
            }
            """;

        var signature = Sign(Timestamp, json);
        var result = DiscordInteractionReceiver.Receive(
            Request(json, ReceivedAt.AddMinutes(2)),
            PublicKeyHex,
            signature,
            Timestamp);

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Equal(MessageRouteKind.Command, result.Route?.Kind);
        Assert.Equal("status", result.Route?.Name);
        Assert.Equal("1", result.Route?.Qualifier);
        Assert.Equal(DiscordApplicationCommandType.ChatInput, result.Envelope?.Payload.CommandType);
        Assert.Equal(MessageEventKind.CommandInvoked, result.Envelope?.Kind);
        Assert.Equal(interactionId, result.Envelope?.EventId);
        Assert.Equal("100000000000000003", result.Envelope?.ScopeId);
        Assert.Equal("100000000000000005", result.Envelope?.SenderId);
        Assert.Equal(MessageConversationKind.Channel, result.Envelope?.Conversation?.ConversationKind);
        Assert.Equal(5, AckType(result.Acknowledgement));
        Assert.Equal(interactionCreatedAt.AddMinutes(15), result.Envelope?.Payload.TransientContext.ExpiresAt);
        Assert.Equal("interaction-secret-token", result.Envelope?.Payload.TransientContext.Token);
        var persisted = JsonSerializer.Serialize(result.Envelope?.Payload);
        Assert.DoesNotContain("interaction-secret-token", persisted, StringComparison.Ordinal);
        using var persistedDocument = JsonDocument.Parse(persisted);
        Assert.Equal("status", persistedDocument.RootElement.GetProperty("Data").GetProperty("name").GetString());
        Assert.False(persistedDocument.RootElement.GetProperty("Data").TryGetProperty("token", out _));
    }

    [Fact]
    public void ComponentDefersUpdateAndCarriesSafeMessageCoordinates() {
        const string json = """
            {
              "id":"100000000000000011","application_id":"100000000000000012","type":3,
              "token":"token-2","channel_id":"100000000000000013","context":1,
              "user":{"id":"100000000000000014"},
              "message":{"id":"100000000000000015"},
              "data":{"custom_id":"approve","component_type":2}
            }
            """;

        var result = Receive(json);

        Assert.Equal(MessageRouteKind.Action, result.Route?.Kind);
        Assert.Equal("approve", result.Route?.Name);
        Assert.Equal(MessageConversationKind.DirectMessage, result.Envelope?.Conversation?.ConversationKind);
        Assert.Equal("100000000000000015", result.Envelope?.Message?.MessageId);
        Assert.Equal(6, AckType(result.Acknowledgement));
    }

    [Fact]
    public void OpaqueComponentIdentifiersPreserveWhitespaceAndDmChannelTypes() {
        const string json = """
            {
              "id":"100000000000000016","application_id":"100000000000000017","type":3,
              "token":"token-opaque","channel_id":"100000000000000018",
              "channel":{"id":"100000000000000018","type":3},
              "user":{"id":"100000000000000019"},
              "data":{"custom_id":" approve ","component_type":2}
            }
            """;

        var result = Receive(json);

        Assert.Equal(" approve ", result.Route?.Name);
        Assert.Equal(" approve ", result.Envelope?.Payload.Name);
        Assert.Equal(MessageConversationKind.DirectMessage, result.Envelope?.Conversation?.ConversationKind);
    }

    [Fact]
    public void ModalAndAutocompleteUseTruthfulDistinctRoutesAndAcknowledgements() {
        const string modal = """
            {
              "id":"100000000000000021","application_id":"100000000000000022","type":5,
              "token":"token-3","user":{"id":"100000000000000023"},
              "data":{"custom_id":"approval","components":[]}
            }
            """;
        const string autocomplete = """
            {
              "id":"100000000000000031","application_id":"100000000000000032","type":4,
              "token":"token-4","user":{"id":"100000000000000033"},
              "data":{"name":"search","type":1,"options":[]}
            }
            """;

        var modalResult = Receive(modal);
        var autocompleteResult = Receive(autocomplete);

        Assert.Equal(MessageRouteKind.Submission, modalResult.Route?.Kind);
        Assert.Equal(MessageEventKind.ModalSubmitted, modalResult.Envelope?.Kind);
        Assert.Equal(5, AckType(modalResult.Acknowledgement));
        Assert.Equal(MessageRouteKind.Autocomplete, autocompleteResult.Route?.Kind);
        Assert.Equal(MessageEventKind.AutocompleteRequested, autocompleteResult.Envelope?.Kind);
        Assert.True(autocompleteResult.RequiresSynchronousDispatch);
        Assert.Equal(8, AckType(autocompleteResult.Acknowledgement));
        using var autocompleteAck = JsonDocument.Parse(autocompleteResult.Acknowledgement.CopyBody());
        Assert.Empty(autocompleteAck.RootElement.GetProperty("data").GetProperty("choices").EnumerateArray());
        var handlerResponse = DiscordInteractionAcknowledgement.Autocomplete(new[] {
            DiscordAutocompleteChoice.FromString("Alpha", "alpha")
        });
        using var response = JsonDocument.Parse(handlerResponse.CopyBody());
        Assert.Equal("alpha", response.RootElement.GetProperty("data").GetProperty("choices")[0].GetProperty("value").GetString());
    }

    [Fact]
    public async Task ComponentAndModalIdentifiersUseExactRoutingAndThreadsRemainThreads() {
        const string json = """
            {
              "id":"100000000000000051","application_id":"100000000000000052","type":3,
              "token":"token","guild_id":"100000000000000053","channel_id":"100000000000000054",
              "channel":{"id":"100000000000000054","type":11},
              "member":{"user":{"id":"100000000000000055"}},
              "message":{"id":"100000000000000056"},
              "data":{"custom_id":"Approve","component_type":2}
            }
            """;
        var result = Receive(json);
        var router = new MessageRouter();
        router.OnAction<DiscordInboundInteraction>("approve", (_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));

        var dispatch = await router.DispatchAsync(
            result.Route!,
            result.Envelope!,
            TestContext.Current.CancellationToken);

        Assert.Equal(MessageConversationKind.Thread, result.Envelope?.Conversation?.ConversationKind);
        Assert.Equal("100000000000000054", result.Envelope?.Conversation?.ThreadId);
        Assert.False(dispatch.RouteMatched);
    }

    [Fact]
    public void SameNamedApplicationCommandTypesHaveDistinctRouteIdentity() {
        const string userCommand = """
            {"id":"100000000000000061","application_id":"100000000000000062","type":2,"token":"t","user":{"id":"100000000000000063"},"data":{"name":"inspect","type":2,"target_id":"100000000000000064"}}
            """;
        const string messageCommand = """
            {"id":"100000000000000071","application_id":"100000000000000072","type":2,"token":"t","channel_id":"100000000000000074","channel":{"id":"100000000000000074","type":1},"user":{"id":"100000000000000073"},"data":{"name":"inspect","type":3,"target_id":"100000000000000075"}}
            """;

        var user = Receive(userCommand);
        var message = Receive(messageCommand);

        Assert.Equal("2", user.Route?.Qualifier);
        Assert.Equal("3", message.Route?.Qualifier);
        Assert.Equal(DiscordApplicationCommandType.User, user.Envelope?.Payload.CommandType);
        Assert.Equal(DiscordApplicationCommandType.Message, message.Envelope?.Payload.CommandType);
        Assert.Equal("100000000000000064", user.Envelope?.Payload.TargetId);
        Assert.Equal("100000000000000075", message.Envelope?.Payload.TargetId);
        Assert.Equal("100000000000000075", message.Envelope?.Message?.MessageId);
        Assert.Equal(MessageConversationKind.DirectMessage, message.Envelope?.Conversation?.ConversationKind);
    }

    [Fact]
    public void MessageCommandRejectsConflictingProviderMessageIdentity() {
        const string json = """
            {"id":"100000000000000071","application_id":"100000000000000072","type":2,"token":"t","channel_id":"100000000000000074","user":{"id":"100000000000000073"},"message":{"id":"100000000000000076"},"data":{"name":"inspect","type":3,"target_id":"100000000000000075"}}
            """;

        var result = Receive(json);

        Assert.Equal(MessageReceiveStatus.Rejected, result.Status);
        Assert.Equal(MessageReceiveFailureKind.Malformed, result.FailureKind);
    }

    [Fact]
    public void SafeInteractionProjectionRoundTripsWithoutTransientCapability() {
        const string json = """
            {"id":"100000000000000091","application_id":"100000000000000092","type":2,"token":"secret","user":{"id":"100000000000000093"},"data":{"name":"inspect","type":2,"target_id":"100000000000000094"}}
            """;
        var result = Receive(json);

        var persisted = JsonSerializer.Serialize(result.Envelope!.Payload);
        var roundTrip = JsonSerializer.Deserialize<DiscordInboundInteraction>(persisted);

        Assert.NotNull(roundTrip);
        Assert.Equal(DiscordInteractionKind.ApplicationCommand, roundTrip.Kind);
        Assert.Equal("inspect", roundTrip.Name);
        Assert.Equal("100000000000000094", roundTrip.TargetId);
        Assert.Equal(MessageDataValueKind.Object, roundTrip.Data.Kind);
        Assert.Equal("100000000000000094", roundTrip.Data.GetProperty("target_id").GetString());
        Assert.Null(roundTrip.TransientContext.Token);
        Assert.DoesNotContain("secret", persisted, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicInteractionProjectionOwnsAndSanitizesProviderData() {
        DiscordInboundInteraction interaction;
        using (var document = JsonDocument.Parse(
                   "{\"name\":\"status\",\"options\":[{\"value\":\"server-1\",\"token\":\"nested-secret\"}],\"resolved\":{\"attachments\":{\"1\":{\"filename\":\"proof.txt\",\"url\":\"https://cdn.example/secret\",\"proxy_url\":\"https://proxy.example/secret\"}}}}")) {
            interaction = new DiscordInboundInteraction(
                DiscordInteractionKind.ApplicationCommand,
                "status",
                null,
                null,
                null,
                null,
                DiscordApplicationCommandType.ChatInput,
                null,
                MessageDataValue.ParseJson(document.RootElement.GetRawText()));
        }

        Assert.Equal("server-1", interaction.Data.GetProperty("options")[0].GetProperty("value").GetString());
        Assert.False(interaction.Data.GetProperty("options")[0].TryGetProperty("token", out _));
        var attachment = interaction.Data.GetProperty("resolved").GetProperty("attachments").GetProperty("1");
        Assert.Equal("proof.txt", attachment.GetProperty("filename").GetString());
        Assert.False(attachment.TryGetProperty("url", out _));
        Assert.False(attachment.TryGetProperty("proxy_url", out _));
        Assert.DoesNotContain("nested-secret", JsonSerializer.Serialize(interaction), StringComparison.Ordinal);
    }

    [Fact]
    public async Task QualifiedDiscordCommandFallsBackToOrdinaryCommandRegistration() {
        const string json = """
            {"id":"100000000000000081","application_id":"100000000000000082","type":2,"token":"t","user":{"id":"100000000000000083"},"data":{"name":"status","type":1}}
            """;
        var result = Receive(json);
        var router = new MessageRouter();
        router.OnCommand<DiscordInboundInteraction>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));

        var dispatch = await router.DispatchAsync(
            result.Route!,
            result.Envelope!,
            TestContext.Current.CancellationToken);

        Assert.Equal("1", result.Route?.Qualifier);
        Assert.True(dispatch.RouteMatched);
    }

    [Fact]
    public void AutocompleteChoicesRejectNumbersOutsideDiscordSafeRange() {
        const long maximum = 9007199254740992L;

        Assert.NotNull(DiscordAutocompleteChoice.FromInteger("maximum", maximum));
        Assert.NotNull(DiscordAutocompleteChoice.FromNumber("minimum", -maximum));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DiscordAutocompleteChoice.FromInteger("too-large", maximum + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DiscordAutocompleteChoice.FromNumber("too-small", -9007199254740994D));
    }

    [Fact]
    public void InvalidSignatureStaleTimestampAndWrongContentTypeFailClosed() {
        const string json = "{\"type\":1}";
        var signature = Sign(Timestamp, json);
        var invalid = DiscordInteractionReceiver.Receive(
            Request(json),
            PublicKeyHex,
            new string('0', 128),
            Timestamp);
        var stale = DiscordInteractionReceiver.Receive(
            Request(json, ReceivedAt.AddMinutes(6)),
            PublicKeyHex,
            signature,
            Timestamp);
        var wrongType = DiscordInteractionReceiver.Receive(
            new MessageInboundRequest("installation-1", "text/plain", Encoding.UTF8.GetBytes(json), ReceivedAt),
            PublicKeyHex,
            signature,
            Timestamp);

        Assert.Equal(MessageReceiveFailureKind.Unauthorized, invalid.FailureKind);
        Assert.Equal(401, invalid.Acknowledgement.StatusCode);
        Assert.Equal(MessageReceiveFailureKind.Unauthorized, stale.FailureKind);
        Assert.Equal(401, stale.Acknowledgement.StatusCode);
        Assert.Equal(MessageReceiveFailureKind.Unsupported, wrongType.FailureKind);
        Assert.Equal(415, wrongType.Acknowledgement.StatusCode);
    }

    [Theory]
    [InlineData("{not-json}")]
    [InlineData("{\"type\":\"2\"}")]
    [InlineData("{\"type\":2,\"id\":\"1\",\"application_id\":\"2\",\"token\":\"t\",\"user\":{\"id\":\"3\"},\"data\":[]}")]
    [InlineData("{\"type\":3,\"id\":\"1\",\"application_id\":\"2\",\"token\":\"t\",\"user\":{\"id\":\"3\"},\"member\":{\"user\":{\"id\":\"4\"}},\"data\":{\"custom_id\":\"a\"}}")]
    [InlineData("{\"type\":2,\"id\":\"1\\n\",\"application_id\":\"2\",\"token\":\"t\",\"user\":{\"id\":\"3\"},\"data\":{\"name\":\"a\"}}")]
    public void MalformedJsonTypesAndConflictingUsersAreRejected(string json) {
        var result = Receive(json);

        Assert.Equal(MessageReceiveFailureKind.Malformed, result.FailureKind);
        Assert.Equal(400, result.Acknowledgement.StatusCode);
    }

    [Fact]
    public void DeduplicationKeyIsScopedToTrustedInstallationRoute() {
        const string json = """
            {"id":"100000000000000041","application_id":"100000000000000042","type":2,"token":"t","user":{"id":"100000000000000043"},"data":{"name":"status","type":1}}
            """;
        var signature = Sign(Timestamp, json);
        var first = DiscordInteractionReceiver.Receive(
            Request(json), PublicKeyHex, signature, Timestamp);
        var second = DiscordInteractionReceiver.Receive(
            new MessageInboundRequest("installation-2", "application/json", Encoding.UTF8.GetBytes(json), ReceivedAt),
            PublicKeyHex, signature, Timestamp);

        Assert.NotEqual(first.Envelope?.DeduplicationKey, second.Envelope?.DeduplicationKey);
    }

    [Fact]
    public void RetryDeduplicationUsesVerifiedInteractionIdentityInsteadOfRequestSignature() {
        var interactionCreatedAt = ReceivedAt.AddMinutes(-1);
        var interactionId = SnowflakeAt(interactionCreatedAt);
        var json = $$$"""
            {"id":"{{{interactionId}}}","application_id":"100000000000000072","type":2,"token":"t","user":{"id":"100000000000000073"},"data":{"name":"status","type":1}}
            """;
        var retryTimestamp = (long.Parse(Timestamp) + 60).ToString();
        var first = DiscordInteractionReceiver.Receive(
            Request(json), PublicKeyHex, Sign(Timestamp, json), Timestamp);
        var retry = DiscordInteractionReceiver.Receive(
            Request(json, ReceivedAt.AddMinutes(1)),
            PublicKeyHex,
            Sign(retryTimestamp, json),
            retryTimestamp);

        Assert.Equal(first.Envelope?.EventId, retry.Envelope?.EventId);
        Assert.Equal(first.Envelope?.DeduplicationKey, retry.Envelope?.DeduplicationKey);
        Assert.Equal(interactionCreatedAt.AddMinutes(15), first.Envelope?.Payload.TransientContext.ExpiresAt);
        Assert.Equal(
            first.Envelope?.Payload.TransientContext.ExpiresAt,
            retry.Envelope?.Payload.TransientContext.ExpiresAt);
    }

    [Fact]
    public void InstallationOwnerSelectionFollowsInteractionContextAndConfiguredOwner() {
        const string direct = """
            {
              "id":"100000000000000051","application_id":"100000000000000052","type":2,
              "token":"token","context":1,"user":{"id":"100000000000000053"},
              "authorizing_integration_owners":{"0":"100000000000000054","1":"100000000000000055"},
              "data":{"name":"status","type":1}
            }
            """;
        var signature = Sign(Timestamp, direct);

        var userOwned = DiscordInteractionReceiver.Receive(
            Request(direct),
            PublicKeyHex,
            signature,
            Timestamp,
            expectedInstallationOwnerId: "100000000000000055");
        var wrongGuildOwner = DiscordInteractionReceiver.Receive(
            Request(direct),
            PublicKeyHex,
            signature,
            Timestamp,
            expectedInstallationOwnerId: "100000000000000054");

        Assert.Equal(MessageReceiveStatus.DispatchReady, userOwned.Status);
        Assert.Equal("100000000000000055", userOwned.Envelope?.Payload.InstallationOwnerId);
        Assert.Equal(MessageReceiveFailureKind.Unauthorized, wrongGuildOwner.FailureKind);
        Assert.Equal(403, wrongGuildOwner.Acknowledgement.StatusCode);
    }

    [Fact]
    public void ExplicitConfiguredOwnerZeroAuthorizesWithoutPersistingSentinel() {
        const string json = """
            {
              "id":"100000000000000061","application_id":"100000000000000062","type":2,
              "token":"token","context":0,"user":{"id":"100000000000000063"},
              "authorizing_integration_owners":{"0":"0"},
              "data":{"name":"status","type":1}
            }
            """;
        var result = DiscordInteractionReceiver.Receive(
            Request(json),
            PublicKeyHex,
            Sign(Timestamp, json),
            Timestamp,
            expectedInstallationOwnerId: "0");

        Assert.Equal(MessageReceiveStatus.DispatchReady, result.Status);
        Assert.Null(result.Envelope?.Payload.InstallationOwnerId);
    }

    private static MessageReceiveResult<DiscordInboundInteraction> Receive(string json) {
        var signature = Sign(Timestamp, json);
        return DiscordInteractionReceiver.Receive(
            Request(json),
            PublicKeyHex,
            signature,
            Timestamp);
    }

    private static MessageInboundRequest Request(string json, DateTimeOffset? receivedAt = null) => new(
        "installation-1",
        "application/json; charset=utf-8",
        Encoding.UTF8.GetBytes(json),
        receivedAt ?? ReceivedAt) {
        CorrelationId = "discord-interaction-test"
    };

    private static string Sign(string timestamp, string json) {
        var body = Encoding.UTF8.GetBytes(json);
        var timestampBytes = Encoding.ASCII.GetBytes(timestamp);
        var signer = new Ed25519Signer();
        signer.Init(true, PrivateKey);
        signer.BlockUpdate(timestampBytes, 0, timestampBytes.Length);
        signer.BlockUpdate(body, 0, body.Length);
        return Convert.ToHexString(signer.GenerateSignature());
    }

    private static string SnowflakeAt(DateTimeOffset timestamp) {
        const long discordEpochMilliseconds = 1420070400000;
        var milliseconds = timestamp.ToUnixTimeMilliseconds() - discordEpochMilliseconds;
        return checked((ulong)milliseconds << 22).ToString();
    }

    private static int AckType(MessageAcknowledgement acknowledgement) {
        using var document = JsonDocument.Parse(acknowledgement.CopyBody());
        return document.RootElement.GetProperty("type").GetInt32();
    }
}
