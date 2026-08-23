using System.Text.Json;
using MessageX.Core;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Persistence.DbaClientX;
using MessageX.Teams.Hosting.AspNetCore;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Core.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace MessageX.Tests;

public sealed class TeamsHostingAdapterTests {
    private static readonly DateTimeOffset ReceivedAt =
        new(2026, 8, 22, 17, 45, 0, TimeSpan.Zero);

    [Fact]
    public void PersonalMessageMapsToDirectRouteAndSafeCoordinates() {
        var dispatch = TeamsActivityMapper.MapMessage(
            Message("personal"),
            "tenant-installation",
            ReceivedAt);

        Assert.Equal(MessageRouteKind.DirectMessage, dispatch.Route.Kind);
        Assert.Equal(MessageEventKind.MessageReceived, dispatch.Envelope.Kind);
        Assert.Equal(MessageProviders.Teams, dispatch.Envelope.Provider);
        Assert.Equal("tenant-installation", dispatch.Envelope.InstallationId);
        Assert.Equal("tenant-1", dispatch.Envelope.ScopeId);
        Assert.Equal("aad-user-1", dispatch.Envelope.SenderId);
        Assert.Equal("conversation-1", dispatch.Envelope.Conversation?.ConversationId);
        Assert.Equal(
            MessageConversationKind.DirectMessage,
            dispatch.Envelope.Conversation?.ConversationKind);
        Assert.Equal("activity-1", dispatch.Envelope.Message?.MessageId);
        Assert.Equal("hello", dispatch.Envelope.Payload.Text);
        Assert.Equal(
            new DateTimeOffset(2026, 8, 22, 17, 30, 0, TimeSpan.Zero),
            dispatch.Envelope.EventTime);
    }

    [Theory]
    [InlineData("groupChat", null, MessageConversationKind.GroupChat)]
    [InlineData("channel", null, MessageConversationKind.Channel)]
    [InlineData("channel", "root-message", MessageConversationKind.Thread)]
    public void ConversationShapeIsPreserved(
        string conversationType,
        string? replyToId,
        MessageConversationKind expectedKind) {
        var activity = Message(conversationType, replyToId);

        var dispatch = TeamsActivityMapper.MapMessage(
            activity,
            "tenant-installation",
            ReceivedAt);

        Assert.Equal(expectedKind, dispatch.Envelope.Conversation?.ConversationKind);
        Assert.Equal(replyToId, dispatch.Envelope.Conversation?.ThreadId);
        Assert.Equal("team-1", dispatch.Envelope.Payload.TeamId);
        Assert.Equal("channel-1", dispatch.Envelope.Payload.ChannelId);
    }

    [Fact]
    public void PersonalChatReplyUsesDirectMessageRouteWhilePreservingReplyShape() {
        var dispatch = TeamsActivityMapper.MapMessage(
            Message("personal", "quoted-message"),
            "tenant-installation",
            ReceivedAt);

        Assert.Equal(MessageRouteKind.DirectMessage, dispatch.Route.Kind);
        Assert.Equal(MessageConversationKind.Thread, dispatch.Envelope.Conversation?.ConversationKind);
        Assert.Equal("quoted-message", dispatch.Envelope.Conversation?.ThreadId);
    }

    [Fact]
    public void UpdateDeleteAndReactionUseTruthfulEventRoutes() {
        var update = TeamsActivityMapper.MapMessageUpdate(
            MessageUpdateActivity.FromActivity(Core("messageUpdate", "\"text\":\"changed\"")),
            "tenant-installation",
            ReceivedAt);
        var delete = TeamsActivityMapper.MapMessageDelete(
            MessageDeleteActivity.FromActivity(Core("messageDelete")),
            "tenant-installation",
            ReceivedAt);
        var reactionSource = Core(
            "messageReaction",
            "\"reactionsAdded\":[{\"type\":\"like\"}],\"reactionsRemoved\":[{\"type\":\"heart\"}]",
            replyToId: "target-message");
        var reaction = TeamsActivityMapper.MapReaction(
            MessageReactionActivity.FromActivity(reactionSource),
            "tenant-installation",
            ReceivedAt,
            reactionSource);

        Assert.Equal(MessageEventKind.MessageChanged, update.Envelope.Kind);
        Assert.Equal(TeamsInboundActivityKind.MessageUpdated, update.Envelope.Payload.Kind);
        Assert.Equal("changed", update.Envelope.Payload.Text);
        Assert.Equal(MessageEventKind.MessageDeleted, delete.Envelope.Kind);
        Assert.Equal(TeamsInboundActivityKind.MessageDeleted, delete.Envelope.Payload.Kind);
        Assert.Equal(MessageEventKind.ReactionChanged, reaction.Envelope.Kind);
        Assert.Equal("target-message", reaction.Envelope.Message?.MessageId);
        Assert.Null(reaction.Envelope.Message?.Timestamp);
        Assert.NotNull(reaction.Envelope.EventTime);
        Assert.Equal(MessageConversationKind.Channel, reaction.Envelope.Conversation?.ConversationKind);
        Assert.Null(reaction.Envelope.Conversation?.ThreadId);
        Assert.Equal(["like"], reaction.Envelope.Payload.ReactionsAdded);
        Assert.Equal(["heart"], reaction.Envelope.Payload.ReactionsRemoved);
    }

    [Fact]
    public void AdaptiveCardActionUsesNamedActionRoute() {
        var dispatch = TeamsActivityMapper.MapAdaptiveCardActionCore(
            Message("channel"),
            "tenant-installation",
            ReceivedAt,
            "approve-request");

        Assert.Equal(MessageRouteKind.Action, dispatch.Route.Kind);
        Assert.Equal("approve-request", dispatch.Route.Name);
        Assert.Equal(MessageEventKind.ActionInvoked, dispatch.Envelope.Kind);
        Assert.Equal("approve-request", dispatch.Envelope.Payload.ActionName);
    }

    [Fact]
    public void AdaptiveCardHandlerAcknowledgementMapsToMicrosoftInvokeResponse() {
        var acknowledgement = new MessageAcknowledgement(
            202,
            "application/json",
            System.Text.Encoding.UTF8.GetBytes("{\"accepted\":true}"));

        var response = TeamsBotApplicationExtensions.CreateInvokeResponse(acknowledgement);

        Assert.Equal(202, response.Status);
        var body = Assert.IsType<JsonElement>(response.Body);
        Assert.True(body.GetProperty("accepted").GetBoolean());
    }

    [Fact]
    public async Task AdaptiveCardDispatchReturnsHandlerAcknowledgementAndTreatsReplayAsSuccess() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IMessageDurableStore>(store);
        services.AddMessageXTeamsHosting();
        services.AddMessageXDurableIngress();
        using var provider = services.BuildServiceProvider();
        var router = provider.GetRequiredService<MessageRouter>();
        router.OnAction<TeamsInboundActivity>("approve-request", (_, _) =>
            Task.FromResult(MessageHandlerResult.Respond(new MessageAcknowledgement(
                202,
                "application/json",
                System.Text.Encoding.UTF8.GetBytes("{\"accepted\":true}")))));
        var dispatch = TeamsActivityMapper.MapAdaptiveCardActionCore(
            Message("channel"),
            "tenant-installation",
            ReceivedAt,
            "approve-request");

        var first = await TeamsBotApplicationExtensions.DispatchAdaptiveCardAsync(
            dispatch,
            provider.GetRequiredService<IMessageIngressAcceptance>(),
            router,
            TestContext.Current.CancellationToken);
        var duplicate = await TeamsBotApplicationExtensions.DispatchAdaptiveCardAsync(
            dispatch,
            provider.GetRequiredService<IMessageIngressAcceptance>(),
            router,
            TestContext.Current.CancellationToken);

        Assert.Equal(202, first?.HandlerResult?.Acknowledgement?.StatusCode);
        Assert.Null(duplicate);
    }

    [Fact]
    public void AdaptiveCardInputsAreBoundedAndSurviveDurableRestoration() {
        var inputs = TeamsActivityMapper.NormalizeAdaptiveInputs(new Dictionary<string, object> {
            ["approved"] = true,
            ["count"] = JsonDocument.Parse("42").RootElement.Clone(),
            ["note"] = "ready"
        });
        var dispatch = TeamsActivityMapper.MapAdaptiveCardActionCore(
            Message("channel"),
            "tenant-installation",
            ReceivedAt,
            "approve-request",
            inputs);
        var codec = new TeamsInboundActivityDurableCodec();

        var restored = codec.Decode(codec.Encode(dispatch.Route, dispatch.Envelope));

        Assert.Equal("true", restored.Payload.InputData["approved"]);
        Assert.Equal("42", restored.Payload.InputData["count"]);
        Assert.Equal("ready", restored.Payload.InputData["note"]);
        Assert.Throws<ArgumentException>(() => TeamsActivityMapper.NormalizeAdaptiveInputs(
            new Dictionary<string, object> { ["nested"] = new { unsafeValue = true } }));
    }

    [Fact]
    public void TransientSdkActivityIsExcludedFromDefaultPersistence() {
        var dispatch = TeamsActivityMapper.MapMessage(
            Message("personal"),
            "tenant-installation",
            ReceivedAt);

        var json = JsonSerializer.Serialize(dispatch.Envelope);

        Assert.DoesNotContain("Activity", json, StringComparison.Ordinal);
        Assert.DoesNotContain("serviceUrl", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("authorization", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant-installation", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TeamsAdapterCommitsThroughDurableAcceptanceAndRestoresSafePayload() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IMessageDurableStore>(store);
        services.AddMessageXTeamsHosting();
        services.AddMessageXDurableIngress();
        using var provider = services.BuildServiceProvider();
        var dispatch = TeamsActivityMapper.MapMessage(
            Message("personal"),
            "tenant-installation",
            ReceivedAt);

        await TeamsBotApplicationExtensions.DispatchAsync(
            dispatch,
            provider.GetRequiredService<IMessageIngressAcceptance>(),
            TestContext.Current.CancellationToken);
        await TeamsBotApplicationExtensions.DispatchAsync(
            dispatch,
            provider.GetRequiredService<IMessageIngressAcceptance>(),
            TestContext.Current.CancellationToken);

        var codec = provider.GetRequiredService<IMessageDurableCodec<TeamsInboundActivity>>();
        var record = codec.Encode(dispatch.Route, dispatch.Envelope);
        var duplicate = await store.AcceptInboxAsync(record, TestContext.Current.CancellationToken);
        var restored = codec.Decode(record);
        Assert.Equal(MessageDurableAcceptanceStatus.AlreadyPending, duplicate.Status);
        Assert.Equal("hello", restored.Payload.Text);
        Assert.Null(restored.Payload.Activity);
        Assert.Equal(dispatch.Envelope.Conversation?.ConversationId, restored.Conversation?.ConversationId);
    }

    [Fact]
    public void InstallationScopesDeduplicationWithoutTrustingActivityCoordinates() {
        var first = TeamsActivityMapper.MapMessage(Message("personal"), "install-a", ReceivedAt);
        var second = TeamsActivityMapper.MapMessage(Message("personal"), "install-b", ReceivedAt);

        Assert.NotEqual(first.Envelope.DeduplicationKey, second.Envelope.DeduplicationKey);
        Assert.Throws<ArgumentException>(() =>
            TeamsActivityMapper.MapMessage(Message("personal"), "install\nunsafe", ReceivedAt));
        Assert.Throws<ArgumentException>(() =>
            TeamsActivityMapper.MapMessage(Message("personal", id: "activity\nunsafe"), "install-a", ReceivedAt));
    }

    [Fact]
    public void MessageWhitespaceIsNotMisclassifiedAsAMentionAndContentKeepsLineBreaks() {
        var activity = Message("channel");
        activity.Text = "  first line\nsecond line  ";

        var dispatch = TeamsActivityMapper.MapMessage(activity, "install-a", ReceivedAt);

        Assert.Equal(MessageRouteKind.Event, dispatch.Route.Kind);
        Assert.Equal("first line\nsecond line", dispatch.Envelope.Payload.Text);
    }

    [Fact]
    public void RecipientMentionUsesMentionRouteAndRemovesOnlyTheBotMention() {
        var activity = MessageActivity.FromActivity(Core(
            "message",
            "\"text\":\"<at>MessageX</at> ask <at>Ada</at> for help\",\"entities\":[{\"type\":\"mention\",\"mentioned\":{\"id\":\"bot-1\"},\"text\":\"<at>MessageX</at>\"},{\"type\":\"mention\",\"mentioned\":{\"id\":\"user-2\"},\"text\":\"<at>Ada</at>\"}]"));

        var dispatch = TeamsActivityMapper.MapMessage(activity, "install-a", ReceivedAt);

        Assert.Equal(MessageRouteKind.Mention, dispatch.Route.Kind);
        Assert.Equal(MessageEventKind.AppMentioned, dispatch.Envelope.Kind);
        Assert.Equal("ask <at>Ada</at> for help", dispatch.Envelope.Payload.Text);
    }

    [Fact]
    public void EditedMessageRemovesOnlyTheBotMention() {
        var source = Core(
            "messageUpdate",
            "\"text\":\"<at>MessageX</at> ask <at>Ada</at>\",\"entities\":[{\"type\":\"mention\",\"mentioned\":{\"id\":\"bot-1\"},\"text\":\"<at>MessageX</at>\"},{\"type\":\"mention\",\"mentioned\":{\"id\":\"user-2\"},\"text\":\"<at>Ada</at>\"}]");
        var activity = MessageUpdateActivity.FromActivity(source);

        var dispatch = TeamsActivityMapper.MapMessageUpdate(
            activity,
            "install-a",
            ReceivedAt,
            source);

        Assert.Equal("ask <at>Ada</at>", dispatch.Envelope.Payload.Text);
    }

    [Fact]
    public void ExistingConversationKindNumbersRemainStable() {
        Assert.Equal(0, (int)MessageConversationKind.Unknown);
        Assert.Equal(1, (int)MessageConversationKind.Channel);
        Assert.Equal(2, (int)MessageConversationKind.DirectMessage);
        Assert.Equal(3, (int)MessageConversationKind.Thread);
        Assert.Equal(4, (int)MessageConversationKind.GroupChat);
    }

    [Fact]
    public void DuplicateHostRegistrationFailsClosed() {
        var application = new object();

        TeamsHostingRegistrationGuard.Register(application);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            TeamsHostingRegistrationGuard.Register(application));
        Assert.Contains("already registered", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdaptedActivityDispatchesThroughTheExactTypedRouteAndHonorsCancellation() {
        var router = new MessageRouter();
        MessageEventEnvelope<TeamsInboundActivity>? observed = null;
        router.OnDirectMessage<TeamsInboundActivity>((context, cancellationToken) => {
            cancellationToken.ThrowIfCancellationRequested();
            observed = context.Envelope;
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var dispatch = TeamsActivityMapper.MapMessage(
            Message("personal"),
            "install-a",
            ReceivedAt);

        var result = await router.DispatchAsync(
            dispatch.Route,
            dispatch.Envelope,
            TestContext.Current.CancellationToken);

        Assert.True(result.RouteMatched);
        Assert.Same(dispatch.Envelope, observed);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            router.DispatchAsync(dispatch.Route, dispatch.Envelope, cancellation.Token));
    }

    [Fact]
    public void VerifiedCoreScopePreservesCoordinatesDroppedByTypedSdkConversion() {
        var source = Core(
            "message",
            "\"text\":\"thread reply\"",
            "channel",
            "root-message");
        var typed = MessageActivity.FromActivity(source);
        Assert.Null(typed.ReplyToId);
        Assert.Null(typed.Timestamp);

        var dispatch = TeamsActivityMapper.MapMessage(
            typed,
            "install-a",
            ReceivedAt,
            source);

        Assert.Equal(MessageConversationKind.Thread, dispatch.Envelope.Conversation?.ConversationKind);
        Assert.Equal("root-message", dispatch.Envelope.Conversation?.ThreadId);
        Assert.Equal(
            new DateTimeOffset(2026, 8, 22, 17, 30, 0, TimeSpan.Zero),
            dispatch.Envelope.EventTime);
    }

    [Fact]
    public void VerifiedCoreScopeRestoresNestedAndCompletedActivityContexts() {
        var outer = Core("message");
        var inner = Core("message", id: "activity-2");
        Assert.Null(TeamsVerifiedActivityScope.Current);

        using (TeamsVerifiedActivityScope.Push(outer)) {
            Assert.Same(outer, TeamsVerifiedActivityScope.Current);
            using (TeamsVerifiedActivityScope.Push(inner)) {
                Assert.Same(inner, TeamsVerifiedActivityScope.Current);
            }
            Assert.Same(outer, TeamsVerifiedActivityScope.Current);
        }

        Assert.Null(TeamsVerifiedActivityScope.Current);
    }

    [Fact]
    public void InstallationResolverReceivesVerifiedCoordinatesPerActivity() {
        var resolver = new TestInstallationResolver();

        var installationId = TeamsBotApplicationExtensions.ResolveInstallation(
            Message("channel"),
            resolver);

        Assert.Equal("tenant-1/team-1/conversation-1", installationId);
        Assert.Equal("tenant-1", resolver.Last?.TenantId);
        Assert.Equal("team-1", resolver.Last?.TeamId);
        Assert.Equal("conversation-1", resolver.Last?.ConversationId);
    }

    private static MessageActivity Message(
        string conversationType,
        string? replyToId = null,
        string id = "activity-1") {
        var activity = MessageActivity.FromActivity(Core(
            "message",
            "\"text\":\"hello\"",
            conversationType,
            replyToId,
            id));
        activity.Timestamp = "2026-08-22T17:30:00Z";
        activity.ReplyToId = replyToId;
        return activity;
    }

    private static CoreActivity Core(
        string type,
        string? additionalJson = null,
        string conversationType = "channel",
        string? replyToId = null,
        string id = "activity-1") {
        var reply = replyToId is null
            ? string.Empty
            : $"\"replyToId\":{JsonSerializer.Serialize(replyToId)},";
        var additional = additionalJson is null ? string.Empty : additionalJson + ",";
        return CoreActivity.FromJsonString($$"""
            {
              "type": {{JsonSerializer.Serialize(type)}},
              "id": {{JsonSerializer.Serialize(id)}},
              "timestamp": "2026-08-22T17:30:00Z",
              {{reply}}
              {{additional}}
              "conversation": {
                "id": "conversation-1",
                "tenantId": "tenant-1",
                "conversationType": {{JsonSerializer.Serialize(conversationType)}}
              },
              "from": { "id": "channel-user-1", "aadObjectId": "aad-user-1" },
              "recipient": { "id": "bot-1" },
              "channelData": {
                "tenant": { "id": "tenant-1" },
                "team": { "id": "team-1" },
                "channel": { "id": "channel-1" }
              },
              "serviceUrl": "https://smba.trafficmanager.net/emea/"
            }
            """);
    }

    private sealed class TestInstallationResolver : ITeamsInstallationResolver {
        public TeamsInstallationContext? Last { get; private set; }

        public string ResolveInstallationId(TeamsInstallationContext context) {
            Last = context;
            return $"{context.TenantId}/{context.TeamId}/{context.ConversationId}";
        }
    }

    private sealed class TemporaryDatabase : IDisposable {
        public TemporaryDatabase() => Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"messagex-teams-{Guid.NewGuid():N}.db");

        public string Path { get; }

        public void Dispose() {
            foreach (var path in new[] { Path, Path + "-wal", Path + "-shm" }) {
                if (File.Exists(path)) {
                    File.Delete(path);
                }
            }
        }
    }
}
