using System.Text.Json;
using MessageX.Core;
using MessageX.Hosting;
using MessageX.Teams.Hosting.AspNetCore;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Core.Schema;

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
    public void UpdateDeleteAndReactionUseTruthfulEventRoutes() {
        var update = TeamsActivityMapper.MapMessageUpdate(
            MessageUpdateActivity.FromActivity(Core("messageUpdate", "\"text\":\"changed\"")),
            "tenant-installation",
            ReceivedAt);
        var delete = TeamsActivityMapper.MapMessageDelete(
            MessageDeleteActivity.FromActivity(Core("messageDelete")),
            "tenant-installation",
            ReceivedAt);
        var reaction = TeamsActivityMapper.MapReaction(
            MessageReactionActivity.FromActivity(Core(
                "messageReaction",
                "\"reactionsAdded\":[{\"type\":\"like\"}],\"reactionsRemoved\":[{\"type\":\"heart\"}]")),
            "tenant-installation",
            ReceivedAt);

        Assert.Equal(MessageEventKind.MessageChanged, update.Envelope.Kind);
        Assert.Equal(TeamsInboundActivityKind.MessageUpdated, update.Envelope.Payload.Kind);
        Assert.Equal("changed", update.Envelope.Payload.Text);
        Assert.Equal(MessageEventKind.MessageDeleted, delete.Envelope.Kind);
        Assert.Equal(TeamsInboundActivityKind.MessageDeleted, delete.Envelope.Payload.Kind);
        Assert.Equal(MessageEventKind.ReactionChanged, reaction.Envelope.Kind);
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
            "\"text\":\"<at>MessageX</at> help\",\"entities\":[{\"type\":\"mention\",\"mentioned\":{\"id\":\"bot-1\"},\"text\":\"<at>MessageX</at>\"}]"));

        var dispatch = TeamsActivityMapper.MapMessage(activity, "install-a", ReceivedAt);

        Assert.Equal(MessageRouteKind.Mention, dispatch.Route.Kind);
        Assert.Equal(MessageEventKind.AppMentioned, dispatch.Envelope.Kind);
        Assert.Equal("help", dispatch.Envelope.Payload.Text);
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
}
