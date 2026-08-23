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
            CorrelationId = " 0HMP1FKC7Q9A3:00000001 ",
            Conversation = new MessageReference(MessageProviders.Slack) {
                ConversationId = "C0123456789"
            }
        };

        Assert.Equal("installation-1", envelope.InstallationId);
        Assert.Equal("event-1", envelope.DeduplicationKey);
        Assert.Equal("provider-event-1", envelope.EventId);
        Assert.Equal("workspace-1", envelope.ScopeId);
        Assert.Equal("user-1", envelope.SenderId);
        Assert.Equal("0HMP1FKC7Q9A3:00000001", envelope.CorrelationId);
        Assert.Equal(receivedAt, envelope.ReceivedAt);
        Assert.Same(payload, envelope.Payload);
        Assert.Equal("C0123456789", envelope.Conversation?.ConversationId);
    }

    [Fact]
    public void DiagnosticTokensAcceptKestrelSeparatorsButRejectUnsafeTransportCharacters() {
        Assert.Equal("trace:00000001", MessageDiagnosticToken.Normalize(" trace:00000001 "));
        Assert.Null(MessageDiagnosticToken.Normalize("trace/00000001"));
        Assert.Null(MessageDiagnosticToken.Normalize("trace\\00000001"));
        Assert.Null(MessageDiagnosticToken.Normalize("trace\r00000001"));
        Assert.Null(MessageDiagnosticToken.Normalize("trace\n00000001"));
        Assert.Null(MessageDiagnosticToken.Normalize(new string('a', 129)));
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

        router.OnAction<TestPayload>(
            "APPROVE",
            (_, _) => Task.FromResult(MessageHandlerResult.Completed()));
        Assert.Throws<InvalidOperationException>(() => router.OnAction<TestPayload>(
            "approve",
            (_, _) => Task.FromResult(MessageHandlerResult.Completed())));
        Assert.Throws<ArgumentException>(() => router.OnCommand<TestPayload>(
            " ",
            (_, _) => Task.FromResult(MessageHandlerResult.Completed())));
        Assert.Throws<ArgumentOutOfRangeException>(() => router.OnEvent<TestPayload>(
            MessageEventKind.Unknown,
            (_, _) => Task.FromResult(MessageHandlerResult.Completed())));
    }

    [Fact]
    public async Task RouterKeepsModalSubmissionsDistinctFromOrdinaryActions() {
        var router = new MessageRouter();
        MessageRouteKind? observedRoute = null;
        router.OnSubmission<TestPayload>("approval", (context, _) => {
            observedRoute = context.Route.Kind;
            return Task.FromResult(MessageHandlerResult.Completed());
        });

        var dispatch = await router.DispatchAsync(
            MessageRoute.ForSubmission("approval"),
            Envelope(new TestPayload("x"), MessageEventKind.ModalSubmitted),
            TestContext.Current.CancellationToken);
        var action = await router.DispatchAsync(
            MessageRoute.ForAction("approval"),
            Envelope(new TestPayload("x"), MessageEventKind.ActionInvoked),
            TestContext.Current.CancellationToken);

        Assert.True(dispatch.RouteMatched);
        Assert.Equal(MessageRouteKind.Submission, observedRoute);
        Assert.True(dispatch.HandlerResult?.Handled);
        Assert.False(action.RouteMatched);
    }

    [Fact]
    public async Task RouterKeepsExactActionIdentifiersAndCommandVariantsIndependent() {
        var router = new MessageRouter();
        router.OnAction<TestPayload>("approve", (_, _) => Task.FromResult(MessageHandlerResult.Completed()));
        router.OnAction<TestPayload>("APPROVE", (_, _) => Task.FromResult(MessageHandlerResult.Ignored()));
        router.OnCommand<TestPayload>("inspect", (_, _) => Task.FromResult(MessageHandlerResult.Ignored()));
        router.OnCommand<TestPayload>("inspect", "2", (_, _) => Task.FromResult(MessageHandlerResult.Completed()));
        router.OnCommand<TestPayload>("inspect", "3", (_, _) => Task.FromResult(MessageHandlerResult.Ignored()));

        var lower = await router.DispatchAsync(
            MessageRoute.ForAction("approve"),
            Envelope(new TestPayload("x"), MessageEventKind.ActionInvoked),
            TestContext.Current.CancellationToken);
        var upper = await router.DispatchAsync(
            MessageRoute.ForAction("APPROVE"),
            Envelope(new TestPayload("x"), MessageEventKind.ActionInvoked),
            TestContext.Current.CancellationToken);
        var userCommand = await router.DispatchAsync(
            MessageRoute.ForCommand("inspect", "2"),
            Envelope(new TestPayload("x"), MessageEventKind.CommandInvoked),
            TestContext.Current.CancellationToken);
        var messageCommand = await router.DispatchAsync(
            MessageRoute.ForCommand("inspect", "3"),
            Envelope(new TestPayload("x"), MessageEventKind.CommandInvoked),
            TestContext.Current.CancellationToken);

        Assert.True(lower.HandlerResult?.Handled);
        Assert.False(upper.HandlerResult?.Handled);
        Assert.True(userCommand.HandlerResult?.Handled);
        Assert.False(messageCommand.HandlerResult?.Handled);
    }

    [Fact]
    public void ExistingEventKindNumbersRemainStable() {
        Assert.Equal(6, (int)MessageEventKind.ReactionChanged);
        Assert.Equal(7, (int)MessageEventKind.MessageChanged);
        Assert.Equal(8, (int)MessageEventKind.MessageDeleted);
        Assert.Equal(9, (int)MessageEventKind.Installed);
        Assert.Equal(10, (int)MessageEventKind.Removed);
        Assert.Equal(11, (int)MessageEventKind.AutocompleteRequested);
    }

    [Fact]
    public async Task RouterKeepsAutocompleteDistinctFromCommandDispatch() {
        var router = new MessageRouter();
        MessageEventKind? observedKind = null;
        router.OnAutocomplete<TestPayload>("search", (context, _) => {
            observedKind = context.Envelope.Kind;
            return Task.FromResult(MessageHandlerResult.Completed());
        });

        var autocomplete = await router.DispatchAsync(
            MessageRoute.ForAutocomplete("SEARCH"),
            Envelope(new TestPayload("x"), MessageEventKind.AutocompleteRequested),
            TestContext.Current.CancellationToken);
        var command = await router.DispatchAsync(
            MessageRoute.ForCommand("search"),
            Envelope(new TestPayload("x"), MessageEventKind.CommandInvoked),
            TestContext.Current.CancellationToken);

        Assert.True(autocomplete.RouteMatched);
        Assert.Equal(MessageEventKind.AutocompleteRequested, observedKind);
        Assert.False(command.RouteMatched);
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

    [Fact]
    public void InboundRequestAndAcknowledgementOwnExactIndependentBodies() {
        var requestBody = new byte[] { 1, 2, 3 };
        var request = new MessageInboundRequest(
            " installation-1 ",
            " application/json ",
            requestBody,
            new DateTimeOffset(2026, 8, 22, 12, 0, 0, TimeSpan.Zero));
        requestBody[0] = 9;
        var firstRead = request.CopyBody();
        firstRead[1] = 9;

        var responseBody = new byte[] { 4, 5, 6 };
        var acknowledgement = new MessageAcknowledgement(200, " application/json ", responseBody);
        responseBody[0] = 9;
        var firstResponseRead = acknowledgement.CopyBody();
        firstResponseRead[1] = 9;

        Assert.Equal(new byte[] { 1, 2, 3 }, request.CopyBody());
        Assert.Equal("installation-1", request.InstallationId);
        Assert.Equal("application/json", request.ContentType);
        Assert.Equal(new byte[] { 4, 5, 6 }, acknowledgement.CopyBody());
        Assert.Equal("application/json", acknowledgement.ContentType);
        Assert.Throws<ArgumentException>(() => new MessageAcknowledgement(
            200,
            "application/json",
            new byte[MessageAcknowledgement.MaximumBodyBytes + 1]));
    }

    [Fact]
    public void ReceiveResultEnforcesRouteAndEnvelopeClassification() {
        var envelope = Envelope(new TestPayload("x"), MessageEventKind.MessageReceived);

        var dispatch = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForDirectMessage(),
            envelope,
            MessageAcknowledgement.Empty(200));

        Assert.Equal(MessageReceiveStatus.DispatchReady, dispatch.Status);
        Assert.Equal(MessageReceiveFailureKind.None, dispatch.FailureKind);
        Assert.Same(envelope, dispatch.Envelope);
        Assert.Throws<ArgumentException>(() => MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            envelope,
            MessageAcknowledgement.Empty(200)));
        Assert.Throws<ArgumentOutOfRangeException>(() => MessageReceiveResult<TestPayload>.Reject(
            MessageReceiveFailureKind.None,
            MessageAcknowledgement.Empty(400)));
    }

    [Fact]
    public void ReceiveResultRetainsThreeArgumentDispatchAbi() {
        var overload = typeof(MessageReceiveResult<TestPayload>)
            .GetMethods()
            .Single(method => method.Name == nameof(MessageReceiveResult<TestPayload>.Dispatch) &&
                method.GetParameters().Length == 3);

        Assert.NotNull(overload);
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
