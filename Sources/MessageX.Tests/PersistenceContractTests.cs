using MessageX.Hosting;

namespace MessageX.Tests;

public sealed class PersistenceContractTests {
    [Theory]
    [InlineData(MessageRouteKind.Event, MessageEventKind.ReactionChanged, null)]
    [InlineData(MessageRouteKind.Command, MessageEventKind.CommandInvoked, "status")]
    [InlineData(MessageRouteKind.Mention, MessageEventKind.AppMentioned, null)]
    [InlineData(MessageRouteKind.DirectMessage, MessageEventKind.MessageReceived, null)]
    [InlineData(MessageRouteKind.Action, MessageEventKind.ActionInvoked, "approve")]
    [InlineData(MessageRouteKind.Submission, MessageEventKind.ModalSubmitted, "approval")]
    [InlineData(MessageRouteKind.Autocomplete, MessageEventKind.AutocompleteRequested, "search")]
    public void DurableRouteCoordinatesRoundTripOnlyTruthfulClassifications(
        MessageRouteKind kind,
        MessageEventKind eventKind,
        string? name) {
        var route = MessageRoute.FromDurableCoordinates(kind, eventKind, name);

        Assert.Equal(kind, route.Kind);
        Assert.Equal(eventKind, route.EventKind);
        Assert.Equal(name, route.Name);
    }

    [Fact]
    public void DurableRouteCoordinatesRejectMismatchedKindsNamesAndEvents() {
        Assert.ThrowsAny<ArgumentException>(() => MessageRoute.FromDurableCoordinates(
            MessageRouteKind.Command,
            MessageEventKind.MessageReceived,
            "status"));
        Assert.ThrowsAny<ArgumentException>(() => MessageRoute.FromDurableCoordinates(
            MessageRouteKind.Mention,
            MessageEventKind.AppMentioned,
            "unexpected"));
        Assert.ThrowsAny<ArgumentException>(() => MessageRoute.FromDurableCoordinates(
            MessageRouteKind.Event,
            MessageEventKind.Unknown));
    }

    [Fact]
    public void DurableInboxRecordOwnsBoundedIndependentSafeProjection() {
        var payload = new byte[] { 1, 2, 3 };
        var record = new MessageDurableRecord(
            MessageProviders.Slack,
            " installation-a ",
            " event-1 ",
            MessageRoute.ForMention(),
            new DateTimeOffset(2026, 8, 22, 19, 0, 0, TimeSpan.Zero),
            " slack.event.v1 ",
            payload);
        payload[0] = 9;
        var copy = record.CopyPayload();
        copy[1] = 9;

        Assert.Equal(new byte[] { 1, 2, 3 }, record.CopyPayload());
        Assert.Equal("installation-a", record.InstallationId);
        Assert.Equal("event-1", record.DeduplicationKey);
        Assert.Equal("slack.event.v1", record.PayloadType);
        Assert.Throws<ArgumentException>(() => new MessageDurableRecord(
            MessageProviders.Slack,
            "installation-a",
            "event-2",
            MessageRoute.ForMention(),
            DateTimeOffset.UtcNow,
            "slack.event.v1",
            new byte[(1024 * 1024) + 1]));
    }

    [Fact]
    public void TransactionalOutboxRecordOwnsBoundedIndependentProjectionWithoutCredentials() {
        var payload = new byte[] { 4, 5, 6 };
        var record = new MessageOutboxRecord(
            MessageProviders.Discord,
            "application-a",
            "reply-event-1",
            "send-message",
            "discord.send.v1",
            payload,
            new DateTimeOffset(2026, 8, 22, 19, 1, 0, TimeSpan.Zero));
        payload[0] = 9;
        var copy = record.CopyPayload();
        copy[1] = 9;

        Assert.Equal(new byte[] { 4, 5, 6 }, record.CopyPayload());
        Assert.Equal("application-a", record.InstallationId);
        Assert.Equal("send-message", record.Operation);
        Assert.DoesNotContain("token", record.GetType().GetProperties().Select(property => property.Name),
            StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", record.GetType().GetProperties().Select(property => property.Name),
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void DurableLeasesRequireOneBasedAttemptsAndOpaqueCoordinates() {
        var record = Record();
        var lease = new MessageDurableLease(
            "record-1",
            "lease-1",
            new DateTimeOffset(2026, 8, 22, 19, 5, 0, TimeSpan.Zero),
            1,
            record);

        Assert.Equal(1, lease.AttemptCount);
        Assert.Same(record, lease.Record);
        Assert.Throws<ArgumentOutOfRangeException>(() => new MessageDurableLease(
            "record-1",
            "lease-1",
            DateTimeOffset.UtcNow,
            0,
            record));
    }

    private static MessageDurableRecord Record() => new(
        MessageProviders.Teams,
        "tenant-a",
        "activity-1",
        MessageRoute.ForDirectMessage(),
        new DateTimeOffset(2026, 8, 22, 19, 0, 0, TimeSpan.Zero),
        "teams.message.v1",
        Array.Empty<byte>());
}
