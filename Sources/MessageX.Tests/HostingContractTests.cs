using MessageX.Hosting;

namespace MessageX.Tests;

public sealed class HostingContractTests {
    [Fact]
    public void EventEnvelopeCarriesVerifiedSafeCoordinatesAndTypedPayload() {
        var receivedAt = new DateTimeOffset(2026, 8, 22, 12, 0, 0, TimeSpan.Zero);
        var payload = new TestPayload("status");
        var envelope = new MessageEventEnvelope<TestPayload>(
            MessageProviders.Slack,
            " installation-1 ",
            " event-1 ",
            MessageEventKind.CommandInvoked,
            receivedAt,
            payload) {
            EventId = " provider-event-1 ",
            ScopeId = " workspace-1 ",
            SenderId = " user-1 ",
            CorrelationId = " trace_1 ",
            Conversation = new MessageReference(MessageProviders.Slack) {
                ConversationId = "C0123456789"
            }
        };

        Assert.Equal("installation-1", envelope.InstallationId);
        Assert.Equal("event-1", envelope.DeduplicationKey);
        Assert.Equal("provider-event-1", envelope.EventId);
        Assert.Equal("workspace-1", envelope.ScopeId);
        Assert.Equal("user-1", envelope.SenderId);
        Assert.Equal("trace_1", envelope.CorrelationId);
        Assert.Equal(receivedAt, envelope.ReceivedAt);
        Assert.Same(payload, envelope.Payload);
        Assert.Equal("C0123456789", envelope.Conversation?.ConversationId);
    }

    [Fact]
    public void EventEnvelopeRejectsMissingUnsafeOrUnboundedCoordinates() {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => new MessageEventEnvelope<TestPayload>(
            " ", "installation", "event", MessageEventKind.MessageReceived, now, new TestPayload("x")));
        Assert.Throws<ArgumentException>(() => new MessageEventEnvelope<TestPayload>(
            MessageProviders.Discord, "installation", "event\n2", MessageEventKind.MessageReceived, now, new TestPayload("x")));
        Assert.Throws<ArgumentException>(() => new MessageEventEnvelope<TestPayload>(
            MessageProviders.Teams, new string('i', 257), "event", MessageEventKind.MessageReceived, now, new TestPayload("x")));
        Assert.Throws<ArgumentNullException>(() => new MessageEventEnvelope<TestPayload>(
            MessageProviders.Teams, "installation", "event", MessageEventKind.MessageReceived, now, null!));
    }

    [Fact]
    public async Task RouterDispatchesNormalizedCommandWithTypedContextAndCancellation() {
        var router = new MessageRouter();
        MessageHandlerContext<TestPayload>? observed = null;
        CancellationToken observedToken = default;
        router.OnCommand<TestPayload>(" status ", (context, cancellationToken) => {
            observed = context;
            observedToken = cancellationToken;
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        using var cancellation = new CancellationTokenSource();
        var envelope = Envelope(new TestPayload("payload"), MessageEventKind.CommandInvoked);

        var dispatch = await router.DispatchAsync(
            MessageRoute.ForCommand("STATUS"),
            envelope,
            cancellation.Token);

        Assert.True(dispatch.RouteMatched);
        Assert.True(dispatch.HandlerResult?.Handled);
        Assert.Same(envelope, observed?.Envelope);
        Assert.Equal("STATUS", observed?.Route.Name);
        Assert.Equal(cancellation.Token, observedToken);
    }

    [Fact]
    public async Task RouterKeepsPayloadTypesAndRoutesIndependent() {
        var router = new MessageRouter();
        router.OnCommand<TestPayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));
        router.OnCommand<OtherPayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.Ignored()));
        router.OnMention<TestPayload>((_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));

        var typed = await router.DispatchAsync(
            MessageRoute.ForCommand("status"),
            Envelope(new TestPayload("x"), MessageEventKind.CommandInvoked),
            TestContext.Current.CancellationToken);
        var other = await router.DispatchAsync(
            MessageRoute.ForCommand("status"),
            Envelope(new OtherPayload(42), MessageEventKind.CommandInvoked),
            TestContext.Current.CancellationToken);
        var missing = await router.DispatchAsync(
            MessageRoute.ForAction("status"),
            Envelope(new TestPayload("x"), MessageEventKind.ActionInvoked),
            TestContext.Current.CancellationToken);

        Assert.True(typed.HandlerResult?.Handled);
        Assert.False(other.HandlerResult?.Handled);
        Assert.False(missing.RouteMatched);
        Assert.Null(missing.HandlerResult);
    }

    [Fact]
    public async Task RouterRejectsRouteThatDisagreesWithVerifiedEventKind() {
        var router = new MessageRouter();
        router.OnCommand<TestPayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => router.DispatchAsync(
            MessageRoute.ForCommand("status"),
            Envelope(new TestPayload("x"), MessageEventKind.MessageReceived),
            TestContext.Current.CancellationToken));

        Assert.Equal("route", exception.ParamName);
    }

    [Fact]
    public void RouterRejectsDuplicateAndInvalidRegistrations() {
        var router = new MessageRouter();
        router.OnAction<TestPayload>("approve", (_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));

        Assert.Throws<InvalidOperationException>(() => router.OnAction<TestPayload>(
            "APPROVE",
            (_, _) => Task.FromResult(MessageHandlerResult.Completed())));
        Assert.Throws<ArgumentException>(() => router.OnCommand<TestPayload>(
            " ",
            (_, _) => Task.FromResult(MessageHandlerResult.Completed())));
        Assert.Throws<ArgumentOutOfRangeException>(() => router.OnEvent<TestPayload>(
            MessageEventKind.Unknown,
            (_, _) => Task.FromResult(MessageHandlerResult.Completed())));
    }

    [Fact]
    public async Task RouterHonorsCancellationBeforeDispatch() {
        var router = new MessageRouter();
        router.OnDirectMessage<TestPayload>((_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => router.DispatchAsync(
            MessageRoute.ForDirectMessage(),
            Envelope(new TestPayload("x"), MessageEventKind.MessageReceived),
            cancellation.Token));
    }

    private static MessageEventEnvelope<TPayload> Envelope<TPayload>(
        TPayload payload,
        MessageEventKind kind) => new(
        MessageProviders.Slack,
        "installation-1",
        "event-1",
        kind,
        new DateTimeOffset(2026, 8, 22, 12, 0, 0, TimeSpan.Zero),
        payload);

    private sealed record TestPayload(string Text);

    private sealed record OtherPayload(int Value);
}
