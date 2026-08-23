using System.Text;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Persistence.DbaClientX;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageX.Tests;

public sealed partial class DurableIngressTests {
    private static readonly DateTimeOffset FixedNow =
        new(2026, 8, 22, 19, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task ProviderSuccessIsWrittenOnlyAfterDurableAcceptanceCommits() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(store, includeCodec: true).BuildServiceProvider();
        var context = ResponseContext();

        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            context.Response,
            Dispatch("accepted"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        var leases = await store.ClaimInboxAsync(
            "test-reader",
            1,
            TimeSpan.FromMinutes(1),
            new[] { "test.payload.v1" },
            TestContext.Current.CancellationToken);
        var lease = Assert.Single(leases);
        Assert.Equal("accepted", Encoding.UTF8.GetString(lease.Record.CopyPayload()));
        Assert.Equal("test.payload.v1", lease.Record.PayloadType);
    }

    [Fact]
    public async Task SynchronousDispatchIsExplicitlyProcessLocalAndCreatesNoDeferredDurableWork() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(store, includeCodec: true).BuildServiceProvider();
        var dispatchCount = 0;
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) => {
            Interlocked.Increment(ref dispatchCount);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var result = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("synchronous"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);
        var processor = provider.GetRequiredService<MessageReceiveResultProcessor>();
        var first = ResponseContext();
        var duplicate = ResponseContext();

        await processor.ProcessAsync(first.Response, result, TestContext.Current.CancellationToken);
        await processor.ProcessAsync(duplicate.Response, result, TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, duplicate.Response.StatusCode);
        Assert.Equal(1, Volatile.Read(ref dispatchCount));
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        Assert.Empty(await store.ClaimInboxAsync(
            "test-reader",
            1,
            TimeSpan.FromMinutes(1),
            new[] { "test.payload.v1" },
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SynchronousDuplicateWithoutAcknowledgementReplayReturnsRetryableFailure() {
        var acceptance = new FixedAcceptance(MessageIngressEnqueueStatus.Duplicate);
        var router = new MessageRouter();
        var calls = 0;
        router.OnCommand<TestPayload>("status", (_, _) => {
            Interlocked.Increment(ref calls);
            return Task.FromResult(MessageHandlerResult.Respond(
                new MessageAcknowledgement(200, "application/json", Encoding.UTF8.GetBytes("{\"ok\":true}"))));
        });
        var processor = new MessageReceiveResultProcessor(
            acceptance,
            new MessageAcknowledgementWriter(),
            router,
            new MessageReplayGuard(1, TimeSpan.FromMinutes(1)));
        var synchronous = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("custom-duplicate"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);
        var response = ResponseContext();

        await processor.ProcessAsync(
            response.Response,
            synchronous,
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.Response.StatusCode);
        Assert.Equal("1", response.Response.Headers.RetryAfter);
        Assert.Equal(0, Volatile.Read(ref calls));
    }

    [Fact]
    public async Task FailedSynchronousDispatchReleasesReplayReservationForProviderRetry() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(store, includeCodec: true).BuildServiceProvider();
        var attempts = 0;
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) => {
            if (Interlocked.Increment(ref attempts) == 1) {
                throw new InvalidOperationException("transient synchronous failure");
            }
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var result = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("synchronous-retry"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);
        var processor = provider.GetRequiredService<MessageReceiveResultProcessor>();
        var first = ResponseContext();
        var retry = ResponseContext();

        await processor.ProcessAsync(first.Response, result, TestContext.Current.CancellationToken);
        await processor.ProcessAsync(retry.Response, result, TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status500InternalServerError, first.Response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, retry.Response.StatusCode);
        Assert.Equal(2, Volatile.Read(ref attempts));
    }

    [Fact]
    public async Task SynchronousCapacityReleasesTheConfiguredAcceptanceReservation() {
        var acceptance = new ReservationOwningAcceptance();
        var gate = new MessageSynchronousDispatchGate(1);
        var router = new MessageRouter();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        router.OnCommand<TestPayload>("status", async (_, cancellationToken) => {
            started.TrySetResult(true);
            await release.Task.WaitAsync(cancellationToken);
            return MessageHandlerResult.Completed();
        });
        var processor = new MessageReceiveResultProcessor(
            acceptance,
            new MessageAcknowledgementWriter(),
            router,
            new MessageReplayGuard(1, TimeSpan.FromMinutes(1)),
            gate);
        var firstResponse = ResponseContext();
        var first = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("custom-reservation-first"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);
        var secondResponse = ResponseContext();
        var second = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("custom-reservation-second"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);

        var firstDispatch = processor.ProcessAsync(
            firstResponse.Response,
            first,
            TestContext.Current.CancellationToken);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await processor.ProcessAsync(
            secondResponse.Response,
            second,
            TestContext.Current.CancellationToken);
        release.TrySetResult(true);
        await firstDispatch;

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, secondResponse.Response.StatusCode);
        Assert.Equal(1, acceptance.ReleaseCount);
    }

    [Fact]
    public async Task SynchronousFailureReleasesTheConfiguredAcceptanceReservation() {
        var acceptance = new ReservationOwningAcceptance();
        var router = new MessageRouter();
        router.OnCommand<TestPayload>("status", (_, _) =>
            throw new InvalidOperationException("handler failure"));
        var processor = new MessageReceiveResultProcessor(
            acceptance,
            new MessageAcknowledgementWriter(),
            router,
            new MessageReplayGuard(1, TimeSpan.FromMinutes(1)),
            new MessageSynchronousDispatchGate(1));
        var response = ResponseContext();
        var result = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("custom-reservation-failure"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);

        await processor.ProcessAsync(
            response.Response,
            result,
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status500InternalServerError, response.Response.StatusCode);
        Assert.Equal(1, acceptance.ReleaseCount);
    }

    [Fact]
    public async Task AcknowledgementWriteFailureDoesNotReleaseSuccessfulSynchronousDispatch() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(store, includeCodec: true).BuildServiceProvider();
        var dispatchCount = 0;
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) => {
            Interlocked.Increment(ref dispatchCount);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var result = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("ack-write-failure"),
            new MessageAcknowledgement(StatusCodes.Status200OK, "application/json", new byte[] { 1 }),
            requiresSynchronousDispatch: true);
        var processor = provider.GetRequiredService<MessageReceiveResultProcessor>();
        var failed = ResponseContext();
        failed.Response.Body = new ThrowingWriteStream();

        await Assert.ThrowsAsync<IOException>(() => processor.ProcessAsync(
            failed.Response,
            result,
            TestContext.Current.CancellationToken));
        var retry = ResponseContext();
        await processor.ProcessAsync(retry.Response, result, TestContext.Current.CancellationToken);

        Assert.Equal(1, Volatile.Read(ref dispatchCount));
        Assert.Equal(StatusCodes.Status200OK, retry.Response.StatusCode);
    }

    [Fact]
    public void ReleasedReplayReservationsDoNotRetainExpirationEntries() {
        using var guard = new MessageReplayGuard(1, TimeSpan.FromHours(1));
        for (var index = 0; index < 1000; index++) {
            var result = Dispatch("released-" + index);
            Assert.Equal(
                MessageReplayAcceptance.Accepted,
                guard.TryAccept(result, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));
            guard.Release(result);
        }
        var field = typeof(MessageReplayGuard).GetField(
            "_expirations",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var value = field?.GetValue(guard);
        var count = value?.GetType().GetProperty("Count")?.GetValue(value);

        Assert.Equal(0, Assert.IsType<int>(count));
    }

    [Fact]
    public async Task ReleasedSynchronousReservationSettlesCurrentAndRacingDuplicatesAsRetryable() {
        using var guard = new MessageReplayGuard(1, TimeSpan.FromHours(1));
        var result = Dispatch("released-synchronous");
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(result, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));
        var currentDuplicate = guard.WaitForAcknowledgementAsync(
            result,
            TestContext.Current.CancellationToken);

        guard.Release(result);

        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            (await currentDuplicate).StatusCode);
        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            (await guard.WaitForAcknowledgementAsync(
                result,
                TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(result, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));
    }

    [Fact]
    public async Task ReplayAcknowledgementBodyBudgetReleasesOverflowForProviderRetry() {
        using var guard = new MessageReplayGuard(
            2,
            TimeSpan.FromHours(1),
            acknowledgementBodyCapacity: 4,
            TimeProvider.System);
        var retained = Dispatch("retained-body");
        var overflow = Dispatch("overflow-body");
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(retained, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));
        guard.Complete(
            retained,
            new MessageAcknowledgement(200, "application/octet-stream", new byte[4]));
        Assert.Equal(
            4,
            (await guard.WaitForAcknowledgementAsync(
                retained,
                TestContext.Current.CancellationToken)).BodyLength);
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(overflow, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));

        guard.Complete(
            overflow,
            new MessageAcknowledgement(200, "application/octet-stream", new byte[1]));

        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            (await guard.WaitForAcknowledgementAsync(
                overflow,
                TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(overflow, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));
    }

    [Fact]
    public void ReplayGuardExpiresCompletedBodiesWithoutSubsequentTraffic() {
        using var guard = new MessageReplayGuard(
            1,
            TimeSpan.FromMilliseconds(25),
            acknowledgementBodyCapacity: 1024,
            TimeProvider.System);
        var result = Dispatch("timer-expiry");
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(
                result,
                TimeProvider.System.GetUtcNow(),
                static () => MessageIngressEnqueueStatus.Accepted));
        guard.Complete(
            result,
            new MessageAcknowledgement(200, "application/octet-stream", new byte[1024]));

        Assert.True(SpinWait.SpinUntil(
            () => ReplayGuardCount(guard) == 0 && ReplayGuardBodyBytes(guard) == 0,
            TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void ReplayGuardChunksPublicRetentionBeyondPlatformTimerLimit() {
        using var guard = new MessageReplayGuard(1, TimeSpan.FromDays(30));
        var accepted = 0;

        var result = guard.TryAccept(
            Dispatch("long-retention"),
            TimeProvider.System.GetUtcNow(),
            () => {
                Interlocked.Increment(ref accepted);
                return MessageIngressEnqueueStatus.Accepted;
            });

        Assert.Equal(MessageReplayAcceptance.Accepted, result);
        Assert.Equal(1, Volatile.Read(ref accepted));
    }

    [Fact]
    public void ReplayGuardPropagatesUnavailableWithoutSuppressingProviderRetry() {
        using var guard = new MessageReplayGuard(1, TimeSpan.FromMinutes(1));
        var result = Dispatch("unavailable-retry");

        Assert.Equal(
            MessageReplayAcceptance.Unavailable,
            guard.TryAccept(result, FixedNow, static () => MessageIngressEnqueueStatus.Unavailable));
        Assert.Equal(
            MessageReplayAcceptance.Accepted,
            guard.TryAccept(result, FixedNow, static () => MessageIngressEnqueueStatus.Accepted));
    }

    [Fact]
    public async Task ReceiveProcessorUsesAcceptanceOwnedSynchronousDispatchGate() {
        var acceptance = new GateOwningAcceptance();
        var router = new MessageRouter();
        var dispatchCount = 0;
        router.OnCommand<TestPayload>("status", (_, _) => {
            Interlocked.Increment(ref dispatchCount);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var processor = new MessageReceiveResultProcessor(
            acceptance,
            new MessageAcknowledgementWriter(),
            router,
            new MessageReplayGuard(1, TimeSpan.FromMinutes(1)),
            new MessageSynchronousDispatchGate(1));
        var response = ResponseContext();
        var result = MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope("acceptance-gate"),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK),
            requiresSynchronousDispatch: true);

        await processor.ProcessAsync(response.Response, result, TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.Response.StatusCode);
        Assert.Equal(1, acceptance.GateEntryCount);
        Assert.Equal(0, Volatile.Read(ref dispatchCount));
    }

    [Fact]
    public async Task MissingOrCoordinateChangingCodecFailsBeforeProviderSuccess() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var missingProvider = Services(store, includeCodec: false).BuildServiceProvider();
        var missing = ResponseContext();

        await missingProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            missing.Response,
            Dispatch("missing"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, missing.Response.StatusCode);
        Assert.Equal("1", missing.Response.Headers.RetryAfter.ToString());
        Assert.Equal(1, missingProvider
            .GetRequiredService<IMessageDurableIngressHealth>()
            .GetHealthSnapshot().Unavailable);

        var services = Services(store, includeCodec: false);
        services.AddMessageXDurableCodec<TestPayload, CoordinateChangingCodec>();
        using var changingProvider = services.BuildServiceProvider();
        var changing = ResponseContext();
        await changingProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            changing.Response,
            Dispatch("changed"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, changing.Response.StatusCode);
        await store.InitializeAsync(TestContext.Current.CancellationToken);
        Assert.Empty(await store.ClaimInboxAsync(
            "test-reader",
            10,
            TimeSpan.FromMinutes(1),
            new[] { "test.payload.v1" },
            TestContext.Current.CancellationToken));

        var missingDispatchServices = Services(store, includeCodec: false);
        missingDispatchServices.AddSingleton<IMessageDurableCodec<TestPayload>, TestPayloadCodec>();
        using var missingDispatchProvider = missingDispatchServices.BuildServiceProvider();
        var missingDispatch = ResponseContext();
        await missingDispatchProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            missingDispatch.Response,
            Dispatch("missing-dispatch"),
            TestContext.Current.CancellationToken);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, missingDispatch.Response.StatusCode);

        var routeServices = Services(store, includeCodec: false);
        routeServices.AddMessageXDurableCodec<TestPayload, RouteChangingCodec>();
        using var routeProvider = routeServices.BuildServiceProvider();
        var changedRoute = ResponseContext();
        await routeProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            changedRoute.Response,
            Dispatch("changed-route"),
            TestContext.Current.CancellationToken);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, changedRoute.Response.StatusCode);

        var actionServices = Services(store, includeCodec: false);
        actionServices.AddMessageXDurableCodec<TestPayload, ActionRouteChangingCodec>();
        using var actionProvider = actionServices.BuildServiceProvider();
        var changedAction = ResponseContext();
        await actionProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            changedAction.Response,
            MessageReceiveResult<TestPayload>.Dispatch(
                MessageRoute.ForAction("approve"),
                new MessageEventEnvelope<TestPayload>(
                    MessageProviders.Slack,
                    "installation-a",
                    "event-changed-action",
                    MessageEventKind.ActionInvoked,
                    FixedNow,
                    new TestPayload("changed-action")),
                MessageAcknowledgement.Empty(StatusCodes.Status200OK)),
            TestContext.Current.CancellationToken);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, changedAction.Response.StatusCode);
    }

    [Fact]
    public async Task DurableAcceptanceWinsRegardlessOfRegistrationOrder() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IMessageDurableStore>(store);
        services.AddMessageXDurableIngress();
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableCodec<TestPayload, TestPayloadCodec>();
        using var provider = services.BuildServiceProvider();
        var response = ResponseContext();

        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            response.Response,
            Dispatch("registration-order"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, response.Response.StatusCode);
        Assert.Single(await store.ClaimInboxAsync(
            "test-reader",
            1,
            TimeSpan.FromMinutes(1),
            new[] { "test.payload.v1" },
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DurableWorkerRecoversAcceptedWorkAfterHostRestart() {
        using var database = new TemporaryDatabase();
        using (var acceptingStore = new SqliteMessageDurableStore(database.Path)) {
            using var acceptingProvider = Services(acceptingStore, includeCodec: true).BuildServiceProvider();
            var context = ResponseContext();
            await acceptingProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
                context.Response,
                Dispatch("restart"),
                TestContext.Current.CancellationToken);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        }

        using var restartedStore = new SqliteMessageDurableStore(database.Path);
        using var restartedProvider = Services(restartedStore, includeCodec: true).BuildServiceProvider();
        var handled = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        restartedProvider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) => {
            handled.TrySetResult(context.Envelope.Payload.Text);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var workers = restartedProvider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal("restart", await handled.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken));
        await WaitUntilCompletedAsync(restartedStore, "event-restart");
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(
            MessageDurableAcceptanceStatus.AlreadyCompleted,
            (await restartedStore.AcceptInboxAsync(
                Record("restart"),
                TestContext.Current.CancellationToken)).Status);
    }

    [Fact]
    public async Task RouteUnmatchedWorkerLeavesWorkForACapableRollingDeploymentPeer() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var olderStore = new CoordinatedReleaseDurableStore(store);
        using var olderProvider = Services(
            olderStore,
            includeCodec: true,
            retryDelay: TimeSpan.Zero).BuildServiceProvider();
        var response = ResponseContext();
        await olderProvider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            response.Response,
            Dispatch("rolling-route"),
            TestContext.Current.CancellationToken);
        var olderWorkers = olderProvider.GetServices<IHostedService>().ToArray();
        foreach (var worker in olderWorkers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }
        await olderStore.Released.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        using var newerProvider = Services(
            store,
            includeCodec: true,
            retryDelay: TimeSpan.Zero).BuildServiceProvider();
        var handled = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        newerProvider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) => {
            handled.TrySetResult(context.Envelope.Payload.Text);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var newerWorkers = newerProvider.GetServices<IHostedService>().ToArray();
        foreach (var worker in newerWorkers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        try {
            Assert.Equal("rolling-route", await handled.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken));
        } finally {
            olderStore.Resume();
        }
        await WaitUntilCompletedAsync(store, "event-rolling-route");
        for (var index = newerWorkers.Length - 1; index >= 0; index--) {
            await newerWorkers[index].StopAsync(TestContext.Current.CancellationToken);
        }
        for (var index = olderWorkers.Length - 1; index >= 0; index--) {
            await olderWorkers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task DurableWorkerRetriesHandlerFailureWithoutLosingTheRecord() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero).BuildServiceProvider();
        var attempts = 0;
        var handled = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) => {
            var attempt = Interlocked.Increment(ref attempts);
            if (attempt == 1) {
                throw new InvalidOperationException("sensitive handler failure");
            }
            handled.TrySetResult(attempt);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var response = ResponseContext();
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            response.Response,
            Dispatch("retry"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(2, await handled.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken));
        await WaitUntilCompletedAsync(store, "event-retry");
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
        Assert.Equal(2, Volatile.Read(ref attempts));
        var health = provider.GetRequiredService<IMessageDurableIngressHealth>().GetHealthSnapshot();
        Assert.Equal(1, health.Accepted);
        Assert.Equal(2, health.Claimed);
        Assert.Equal(1, health.Retried);
        Assert.Equal(1, health.Completed);
        Assert.DoesNotContain("sensitive handler failure", health.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandlerOutboxCommitsWithInboxAndIsDeliveredByRegisteredOwner() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero);
        services.AddMessageXOutboxHandler<TestOutboxHandler>();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) => {
            var outbound = new MessageOutboxRecord(
                context.Envelope.Provider,
                context.Envelope.InstallationId,
                context.Envelope.DeduplicationKey + ":reply",
                "reply",
                "test.outbox.v1",
                Encoding.UTF8.GetBytes("safe-reply"),
                FixedNow);
            return Task.FromResult(MessageHandlerResult.CompletedWithOutbox(
                new MessageOutboxBatch(new[] { outbound })));
        });
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("outbox"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        var handler = Assert.IsType<TestOutboxHandler>(
            provider.GetServices<IMessageOutboxHandler>().Single());
        Assert.Equal("safe-reply", await handler.Delivered.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken));
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task SynchronousOutboxFailureRetriesWithoutStoppingOtherDeliveries() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero);
        services.AddMessageXOutboxHandler<SynchronousRetryOutboxHandler>();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) => {
            var records = new[] { "retry", "continue" }
                .Select(value => new MessageOutboxRecord(
                    context.Envelope.Provider,
                    context.Envelope.InstallationId,
                    context.Envelope.DeduplicationKey + ":" + value,
                    value,
                    "test.sync-retry.v1",
                    Encoding.UTF8.GetBytes(value),
                    FixedNow));
            return Task.FromResult(MessageHandlerResult.CompletedWithOutbox(
                new MessageOutboxBatch(records)));
        });
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("sync-outbox"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        var handler = Assert.IsType<SynchronousRetryOutboxHandler>(
            provider.GetServices<IMessageOutboxHandler>().Single());
        await handler.Continued.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await handler.Retried.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(2, handler.RetryAttempts);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task AmbiguousOutboxFailureIsNotAutomaticallyRetried() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero);
        services.AddMessageXOutboxHandler<AmbiguousOutboxHandler>();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) =>
            Task.FromResult(MessageHandlerResult.CompletedWithOutbox(new MessageOutboxBatch([
                new MessageOutboxRecord(
                    context.Envelope.Provider,
                    context.Envelope.InstallationId,
                    context.Envelope.DeduplicationKey + ":ambiguous",
                    "send",
                    "test.ambiguous.v1",
                    Array.Empty<byte>(),
                    FixedNow)
            ]))));
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("ambiguous-outbox"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        var handler = Assert.IsType<AmbiguousOutboxHandler>(
            provider.GetServices<IMessageOutboxHandler>().Single());
        await handler.Attempted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.Attempts);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task EveryOutboxBatchLeaseStartsBeforeASynchronousHandlerCanBlock() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        var services = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero);
        services.AddMessageXOutboxHandler<SynchronousBlockingOutboxHandler>();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) => {
            var records = new[] { "block", "continue" }
                .Select(value => new MessageOutboxRecord(
                    context.Envelope.Provider,
                    context.Envelope.InstallationId,
                    context.Envelope.DeduplicationKey + ":" + value,
                    value,
                    "test.sync-block.v1",
                    Encoding.UTF8.GetBytes(value),
                    FixedNow));
            return Task.FromResult(MessageHandlerResult.CompletedWithOutbox(
                new MessageOutboxBatch(records)));
        });
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("sync-block-outbox"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        var handler = Assert.IsType<SynchronousBlockingOutboxHandler>(
            provider.GetServices<IMessageOutboxHandler>().Single());
        await handler.BothEntered.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task DurableInboxWorkerRecoversAfterTransientStoreFailure() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var store = new TransientClaimFailureStore(innerStore, failInbox: true, failOutbox: false);
        using var provider = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero)
            .BuildServiceProvider();
        var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) => {
            handled.TrySetResult(true);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("transient-store"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        await handled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, store.ClaimFailures);
        Assert.True(provider.GetRequiredService<IMessageDurableIngressHealth>()
            .GetHealthSnapshot().Unavailable > 0);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task DurableOutboxWorkerRecoversAfterTransientStoreFailure() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var store = new TransientClaimFailureStore(innerStore, failInbox: false, failOutbox: true);
        var services = Services(store, includeCodec: true, retryDelay: TimeSpan.Zero);
        services.AddMessageXOutboxHandler<TestOutboxHandler>();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.CompletedWithOutbox(new MessageOutboxBatch(new[] {
                new MessageOutboxRecord(
                    MessageProviders.Slack,
                    "installation-a",
                    "outbox-transient-store",
                    "send",
                    "test.outbox.v1",
                    Encoding.UTF8.GetBytes("delivered"),
                    FixedNow)
            }))));
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("transient-outbox-store"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        var handler = Assert.IsType<TestOutboxHandler>(provider.GetServices<IMessageOutboxHandler>().Single());
        Assert.Equal(
            "delivered",
            await handler.Delivered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.Equal(1, store.OutboxClaimFailures);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task LongHandlerRenewsLeaseBeforeAnotherWorkerCanClaimIt() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(
            store,
            includeCodec: true,
            timeProvider: TimeProvider.System,
            leaseDuration: TimeSpan.FromSeconds(1)).BuildServiceProvider();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", async (_, token) => {
            started.TrySetResult(true);
            await release.Task.WaitAsync(token);
            return MessageHandlerResult.Completed();
        });
        var response = ResponseContext();
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            response.Response,
            Dispatch("long"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await Task.Delay(TimeSpan.FromMilliseconds(1400), TestContext.Current.CancellationToken);
        Assert.True(provider
            .GetRequiredService<IMessageDurableIngressHealth>()
            .GetHealthSnapshot().LeaseRenewed > 0);
        Assert.Empty(await store.ClaimInboxAsync(
            "competing-worker",
            1,
            TimeSpan.FromSeconds(1),
            new[] { "test.payload.v1" },
            TestContext.Current.CancellationToken));
        release.TrySetResult(true);
        await WaitUntilCompletedAsync(store, "event-long");
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task EveryClaimedBatchItemStartsDispatchAndRenewalWithoutLeaseStarvation() {
        using var database = new TemporaryDatabase();
        using var store = new SqliteMessageDurableStore(database.Path);
        using var provider = Services(
            store,
            includeCodec: true,
            timeProvider: TimeProvider.System,
            leaseDuration: TimeSpan.FromSeconds(1)).BuildServiceProvider();
        var started = 0;
        var secondStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstTimedOut = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, token) => {
            var ordinal = Interlocked.Increment(ref started);
            if (ordinal == 1 && !secondStarted.Task.Wait(TimeSpan.FromSeconds(2), token)) {
                firstTimedOut.TrySetResult(true);
            } else if (ordinal == 2) {
                secondStarted.TrySetResult(true);
            }
            return AwaitReleaseAsync(release.Task, token);
        });
        var processor = provider.GetRequiredService<MessageReceiveResultProcessor>();
        await processor.ProcessAsync(
            ResponseContext().Response,
            Dispatch("batch-a"),
            TestContext.Current.CancellationToken);
        await processor.ProcessAsync(
            ResponseContext().Response,
            Dispatch("batch-b"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(firstTimedOut.Task.IsCompleted);
        release.TrySetResult(true);
        await WaitUntilCompletedAsync(store, "event-batch-a");
        await WaitUntilCompletedAsync(store, "event-batch-b");
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private static async Task<MessageHandlerResult> AwaitReleaseAsync(
        Task release,
        CancellationToken cancellationToken) {
        await release.WaitAsync(cancellationToken);
        return MessageHandlerResult.Completed();
    }

    [Fact]
    public async Task LeaseLossCancelsHandlerWithoutStaleMutationAndRestartRecovers() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var losingStore = new LeaseLosingStore(innerStore);
        using (var provider = Services(
            losingStore,
            includeCodec: true,
            timeProvider: TimeProvider.System,
            leaseDuration: TimeSpan.FromSeconds(1)).BuildServiceProvider()) {
            var canceled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", async (_, token) => {
                try {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                } catch (OperationCanceledException) when (token.IsCancellationRequested) {
                    canceled.TrySetResult(true);
                    throw;
                }
                return MessageHandlerResult.Completed();
            });
            var response = ResponseContext();
            await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
                response.Response,
                Dispatch("lease-loss"),
                TestContext.Current.CancellationToken);
            var workers = provider.GetServices<IHostedService>().ToArray();
            foreach (var worker in workers) {
                await worker.StartAsync(TestContext.Current.CancellationToken);
            }

            await canceled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.True(losingStore.RenewInboxCalls > 0);
            Assert.Equal(0, losingStore.CompleteInboxCalls);
            Assert.Equal(0, losingStore.FailInboxCalls);
            Assert.True(provider
                .GetRequiredService<IMessageDurableIngressHealth>()
                .GetHealthSnapshot().LeaseLost > 0);
            for (var index = workers.Length - 1; index >= 0; index--) {
                await workers[index].StopAsync(TestContext.Current.CancellationToken);
            }
        }

        await Task.Delay(TimeSpan.FromMilliseconds(1100), TestContext.Current.CancellationToken);
        using var restartedStore = new SqliteMessageDurableStore(database.Path);
        using var restartedProvider = Services(
            restartedStore,
            includeCodec: true,
            timeProvider: TimeProvider.System,
            leaseDuration: TimeSpan.FromSeconds(1)).BuildServiceProvider();
        var recovered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        restartedProvider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) => {
            recovered.TrySetResult(true);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var restartedWorkers = restartedProvider.GetServices<IHostedService>().ToArray();
        foreach (var worker in restartedWorkers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }
        await recovered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await WaitUntilCompletedAsync(restartedStore, "event-lease-loss");
        for (var index = restartedWorkers.Length - 1; index >= 0; index--) {
            await restartedWorkers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private static ServiceCollection Services(
        IMessageDurableStore store,
        bool includeCodec,
        TimeSpan? retryDelay = null,
        TimeProvider? timeProvider = null,
        TimeSpan? leaseDuration = null) {
        var services = new ServiceCollection();
        services.AddSingleton(timeProvider ?? new FixedTimeProvider(FixedNow));
        services.AddSingleton(store);
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableIngress(options => {
            options.PollInterval = TimeSpan.FromMilliseconds(10);
            options.RetryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
            options.MaximumAttempts = 3;
            options.LeaseDuration = leaseDuration ?? TimeSpan.FromMinutes(1);
        });
        if (includeCodec) {
            services.AddMessageXDurableCodec<TestPayload, TestPayloadCodec>();
        }
        return services;
    }

    private static MessageReceiveResult<TestPayload> Dispatch(string text) =>
        MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            Envelope(text),
            MessageAcknowledgement.Empty(StatusCodes.Status200OK));

    private static int ReplayGuardCount(MessageReplayGuard guard) {
        var field = typeof(MessageReplayGuard).GetField(
            "_accepted",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var value = field?.GetValue(guard);
        return Assert.IsType<int>(value?.GetType().GetProperty("Count")?.GetValue(value));
    }

    private static int ReplayGuardBodyBytes(MessageReplayGuard guard) {
        var field = typeof(MessageReplayGuard).GetField(
            "_acknowledgementBodyBytes",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return Assert.IsType<int>(field?.GetValue(guard));
    }

    private static MessageEventEnvelope<TestPayload> Envelope(string text) => new(
        MessageProviders.Slack,
        "installation-a",
        $"event-{text}",
        MessageEventKind.CommandInvoked,
        FixedNow,
        new TestPayload(text));

    private static MessageDurableRecord Record(string text) => new(
        MessageProviders.Slack,
        "installation-a",
        $"event-{text}",
        MessageRoute.ForCommand("status"),
        FixedNow,
        "test.payload.v1",
        Encoding.UTF8.GetBytes(text));

    private static DefaultHttpContext ResponseContext() {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task WaitUntilCompletedAsync(
        IMessageDurableStore store,
        string deduplicationKey) {
        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < timeoutAt) {
            var acceptance = await store.AcceptInboxAsync(
                new MessageDurableRecord(
                    MessageProviders.Slack,
                    "installation-a",
                    deduplicationKey,
                    MessageRoute.ForCommand("status"),
                    FixedNow,
                    "test.payload.v1",
                    Array.Empty<byte>()),
                TestContext.Current.CancellationToken);
            if (acceptance.Status == MessageDurableAcceptanceStatus.AlreadyCompleted) {
                return;
            }
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("Durable work did not reach completed state.");
    }

    private sealed record TestPayload(string Text);

    private sealed class FixedAcceptance(MessageIngressEnqueueStatus status) : IMessageIngressAcceptance {
        public ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
            MessageReceiveResult<TProviderPayload> result,
            CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(status);
        }
    }

    private sealed class ReservationOwningAcceptance :
        IMessageIngressAcceptance,
        IMessageIngressReservationRelease {
        public int ReleaseCount { get; private set; }

        public ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
            MessageReceiveResult<TProviderPayload> result,
            CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(MessageIngressEnqueueStatus.Accepted);
        }

        public void Release<TProviderPayload>(MessageReceiveResult<TProviderPayload> result) {
            ArgumentNullException.ThrowIfNull(result);
            ReleaseCount++;
        }
    }

    private sealed class GateOwningAcceptance :
        IMessageIngressAcceptance,
        IMessageSynchronousDispatchGate {
        public int GateEntryCount { get; private set; }

        public ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
            MessageReceiveResult<TProviderPayload> result,
            CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(MessageIngressEnqueueStatus.Accepted);
        }

        public IDisposable? TryEnterSynchronousDispatch() {
            GateEntryCount++;
            return null;
        }
    }

    private sealed class TestOutboxHandler : IMessageOutboxHandler {
        public string PayloadType => "test.outbox.v1";

        public TaskCompletionSource<string> Delivered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            Delivered.TrySetResult(Encoding.UTF8.GetString(record.CopyPayload()));
            return Task.CompletedTask;
        }
    }

    private sealed class SynchronousRetryOutboxHandler : IMessageOutboxHandler {
        private int _retryAttempts;

        public string PayloadType => "test.sync-retry.v1";

        public TaskCompletionSource<bool> Continued { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Retried { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int RetryAttempts => Volatile.Read(ref _retryAttempts);

        public Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            var value = Encoding.UTF8.GetString(record.CopyPayload());
            if (string.Equals(value, "retry", StringComparison.Ordinal) &&
                Interlocked.Increment(ref _retryAttempts) == 1) {
                throw new MessageOutboxDeliveryException(
                    "synchronous retry contract",
                    MessageOutboxDeliveryOutcome.DefinitelyNotSent);
            }
            if (string.Equals(value, "retry", StringComparison.Ordinal)) {
                Retried.TrySetResult(true);
            } else {
                Continued.TrySetResult(true);
            }
            return Task.CompletedTask;
        }
    }

    private sealed class AmbiguousOutboxHandler : IMessageOutboxHandler {
        private int _attempts;

        public string PayloadType => "test.ambiguous.v1";

        public TaskCompletionSource<bool> Attempted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Attempts => Volatile.Read(ref _attempts);

        public Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _attempts);
            Attempted.TrySetResult(true);
            throw new InvalidOperationException("provider acceptance is unknown");
        }
    }

    private sealed class SynchronousBlockingOutboxHandler : IMessageOutboxHandler {
        private readonly ManualResetEventSlim _continued = new(false);

        public string PayloadType => "test.sync-block.v1";

        public TaskCompletionSource<bool> BothEntered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            var value = Encoding.UTF8.GetString(record.CopyPayload());
            if (string.Equals(value, "continue", StringComparison.Ordinal)) {
                _continued.Set();
            } else if (!_continued.Wait(TimeSpan.FromSeconds(3), cancellationToken)) {
                throw new TimeoutException("A later outbox lease never reached its handler.");
            }
            BothEntered.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    private sealed class TestPayloadCodec : IMessageDurableCodec<TestPayload> {
        public string PayloadType => "test.payload.v1";

        public MessageDurableRecord Encode(
            MessageRoute route,
            MessageEventEnvelope<TestPayload> envelope) => new(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            route,
            envelope.ReceivedAt,
            PayloadType,
            Encoding.UTF8.GetBytes(envelope.Payload.Text));

        public MessageEventEnvelope<TestPayload> Decode(MessageDurableRecord record) => new(
            record.Provider,
            record.InstallationId,
            record.DeduplicationKey,
            record.Route.EventKind,
            record.ReceivedAt,
            new TestPayload(Encoding.UTF8.GetString(record.CopyPayload())));
    }

    private sealed class CoordinateChangingCodec : IMessageDurableCodec<TestPayload> {
        public string PayloadType => "test.payload.changed.v1";

        public MessageDurableRecord Encode(
            MessageRoute route,
            MessageEventEnvelope<TestPayload> envelope) => new(
            envelope.Provider,
            "another-installation",
            envelope.DeduplicationKey,
            route,
            envelope.ReceivedAt,
            PayloadType,
            Array.Empty<byte>());

        public MessageEventEnvelope<TestPayload> Decode(MessageDurableRecord record) =>
            throw new NotSupportedException();
    }

    private sealed class RouteChangingCodec : IMessageDurableCodec<TestPayload> {
        public string PayloadType => "test.payload.route-changed.v1";

        public MessageDurableRecord Encode(
            MessageRoute route,
            MessageEventEnvelope<TestPayload> envelope) => new(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            MessageRoute.ForCommand(route.Name!, "chat-input"),
            envelope.ReceivedAt,
            PayloadType,
            Array.Empty<byte>());

        public MessageEventEnvelope<TestPayload> Decode(MessageDurableRecord record) =>
            throw new NotSupportedException();
    }

    private sealed class ActionRouteChangingCodec : IMessageDurableCodec<TestPayload> {
        public string PayloadType => "test.payload.action-route-changed.v1";

        public MessageDurableRecord Encode(
            MessageRoute route,
            MessageEventEnvelope<TestPayload> envelope) => new(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            MessageRoute.ForAction(route.Name!.ToUpperInvariant()),
            envelope.ReceivedAt,
            PayloadType,
            Array.Empty<byte>());

        public MessageEventEnvelope<TestPayload> Decode(MessageDurableRecord record) =>
            throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider : TimeProvider {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    private sealed class ThrowingWriteStream : MemoryStream {
        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException(new IOException("simulated acknowledgement write failure"));
    }

    private sealed class TransientClaimFailureStore : IMessageDurableStore {
        private readonly IMessageDurableStore _inner;
        private int _remainingInboxFailures;
        private int _remainingOutboxFailures;

        public TransientClaimFailureStore(IMessageDurableStore inner, bool failInbox, bool failOutbox) {
            _inner = inner;
            _remainingInboxFailures = failInbox ? 1 : 0;
            _remainingOutboxFailures = failOutbox ? 1 : 0;
        }

        public int ClaimFailures { get; private set; }

        public int OutboxClaimFailures { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken = default) =>
            _inner.InitializeAsync(cancellationToken);

        public Task<MessageDurableAcceptance> AcceptInboxAsync(
            MessageDurableRecord record,
            CancellationToken cancellationToken = default) =>
            _inner.AcceptInboxAsync(record, cancellationToken);

        public Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) {
            if (Interlocked.Exchange(ref _remainingInboxFailures, 0) == 1) {
                ClaimFailures++;
                throw new InvalidOperationException("transient store outage");
            }
            return _inner.ClaimInboxAsync(
                ownerId,
                maximumCount,
                leaseDuration,
                payloadTypes,
                cancellationToken);
        }

        public Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) =>
            _inner.RenewInboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);

        public Task<bool> ReleaseInboxAsync(
            string recordId,
            string leaseToken,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default) =>
            _inner.ReleaseInboxAsync(recordId, leaseToken, retryDelay, cancellationToken);

        public Task<bool> CompleteInboxAsync(
            string recordId,
            string leaseToken,
            MessageOutboxBatch? outbox = null,
            CancellationToken cancellationToken = default) =>
            _inner.CompleteInboxAsync(recordId, leaseToken, outbox, cancellationToken);

        public Task<MessageDurableFailureResult> FailInboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) =>
            _inner.FailInboxAsync(
                recordId,
                leaseToken,
                failureKind,
                retryDelay,
                maximumAttempts,
                cancellationToken);

        public Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) {
            if (Interlocked.Exchange(ref _remainingOutboxFailures, 0) == 1) {
                OutboxClaimFailures++;
                throw new InvalidOperationException("transient outbox store outage");
            }
            return _inner.ClaimOutboxAsync(ownerId, maximumCount, leaseDuration, payloadTypes, cancellationToken);
        }

        public Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) =>
            _inner.RenewOutboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);

        public Task<bool> CompleteOutboxAsync(
            string recordId,
            string leaseToken,
            CancellationToken cancellationToken = default) =>
            _inner.CompleteOutboxAsync(recordId, leaseToken, cancellationToken);

        public Task<MessageDurableFailureResult> FailOutboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) =>
            _inner.FailOutboxAsync(
                recordId,
                leaseToken,
                failureKind,
                retryDelay,
                maximumAttempts,
                cancellationToken);

        public Task<int> PurgeTerminalAsync(
            DateTimeOffset completedBefore,
            int maximumCount,
            CancellationToken cancellationToken = default) =>
            _inner.PurgeTerminalAsync(completedBefore, maximumCount, cancellationToken);
    }

    private sealed class TemporaryDatabase : IDisposable {
        public TemporaryDatabase() => Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"messagex-ingress-{Guid.NewGuid():N}.db");

        public string Path { get; }

        public void Dispose() => TemporaryPathCleanup.DeleteSqliteDatabase(Path);
    }
}
