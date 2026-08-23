using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Persistence.DbaClientX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageX.Tests;

public sealed partial class DurableIngressTests {
    [Fact]
    public async Task InboxRenewalUsesStoreReportedDurationWithoutComparingClocks() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var losingStore = new LeaseLosingStore(
            innerStore,
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromHours(-1));
        using var provider = Services(
            losingStore,
            includeCodec: true,
            timeProvider: TimeProvider.System,
            leaseDuration: TimeSpan.FromSeconds(30)).BuildServiceProvider();
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
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("short-reported-lease"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        await canceled.Task.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        Assert.True(losingStore.RenewInboxCalls > 0);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task OutboxRenewalUsesStoreReportedDurationWithoutComparingClocks() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var losingStore = new LeaseLosingStore(
            innerStore,
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromHours(-1));
        var services = Services(
            losingStore,
            includeCodec: true,
            timeProvider: TimeProvider.System,
            leaseDuration: TimeSpan.FromSeconds(30));
        services.AddMessageXOutboxHandler<CancellableOutboxHandler>();
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (context, _) =>
            Task.FromResult(MessageHandlerResult.CompletedWithOutbox(new MessageOutboxBatch(new[] {
                new MessageOutboxRecord(
                    context.Envelope.Provider,
                    context.Envelope.InstallationId,
                    context.Envelope.DeduplicationKey + ":cancel",
                    "send",
                    "test.cancellable.v1",
                    Array.Empty<byte>(),
                    FixedNow)
            }))));
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response,
            Dispatch("short-reported-outbox-lease"),
            TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }

        var handler = Assert.IsType<CancellableOutboxHandler>(
            provider.GetServices<IMessageOutboxHandler>().Single());
        await handler.Canceled.Task.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        Assert.True(losingStore.RenewOutboxCalls > 0);
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed class CancellableOutboxHandler : IMessageOutboxHandler {
        public string PayloadType => "test.cancellable.v1";

        public TaskCompletionSource<bool> Canceled { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) {
            try {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                Canceled.TrySetResult(true);
                throw;
            }
        }
    }

    private sealed class LeaseLosingStore : IMessageDurableStore {
        private readonly IMessageDurableStore _inner;
        private readonly TimeSpan? _reportedLeaseLifetime;
        private readonly TimeSpan _reportedClockOffset;

        public LeaseLosingStore(
            IMessageDurableStore inner,
            TimeSpan? reportedLeaseLifetime = null,
            TimeSpan? reportedClockOffset = null) {
            _inner = inner;
            _reportedLeaseLifetime = reportedLeaseLifetime;
            _reportedClockOffset = reportedClockOffset ?? TimeSpan.Zero;
        }

        public int RenewInboxCalls { get; private set; }

        public int RenewOutboxCalls { get; private set; }

        public int CompleteInboxCalls { get; private set; }

        public int FailInboxCalls { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken = default) =>
            _inner.InitializeAsync(cancellationToken);

        public Task<MessageDurableAcceptance> AcceptInboxAsync(
            MessageDurableRecord record,
            CancellationToken cancellationToken = default) =>
            _inner.AcceptInboxAsync(record, cancellationToken);

        public async Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) {
            var leases = await _inner.ClaimInboxAsync(
                ownerId,
                maximumCount,
                leaseDuration,
                payloadTypes,
                cancellationToken);
            if (_reportedLeaseLifetime is null) {
                return leases;
            }
            var reportedExpiry = DateTimeOffset.UtcNow
                .Add(_reportedClockOffset)
                .Add(_reportedLeaseLifetime.Value);
            return leases.Select(lease => new MessageDurableLease(
                lease.RecordId,
                lease.LeaseToken,
                reportedExpiry,
                lease.AttemptCount,
                lease.Record,
                _reportedLeaseLifetime)).ToArray();
        }

        public Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) {
            RenewInboxCalls++;
            return Task.FromResult<MessageLeaseRenewal?>(null);
        }

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
            CancellationToken cancellationToken = default) {
            CompleteInboxCalls++;
            return _inner.CompleteInboxAsync(recordId, leaseToken, outbox, cancellationToken);
        }

        public Task<MessageDurableFailureResult> FailInboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) {
            FailInboxCalls++;
            return _inner.FailInboxAsync(
                recordId,
                leaseToken,
                failureKind,
                retryDelay,
                maximumAttempts,
                cancellationToken);
        }

        public async Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) {
            var leases = await _inner.ClaimOutboxAsync(
                ownerId,
                maximumCount,
                leaseDuration,
                payloadTypes,
                cancellationToken);
            if (_reportedLeaseLifetime is null) {
                return leases;
            }
            var reportedExpiry = DateTimeOffset.UtcNow
                .Add(_reportedClockOffset)
                .Add(_reportedLeaseLifetime.Value);
            return leases.Select(lease => new MessageOutboxLease(
                lease.RecordId,
                lease.LeaseToken,
                reportedExpiry,
                lease.AttemptCount,
                lease.Record,
                _reportedLeaseLifetime)).ToArray();
        }

        public Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) {
            RenewOutboxCalls++;
            return Task.FromResult<MessageLeaseRenewal?>(null);
        }

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
            TimeSpan terminalRetention,
            int maximumCount,
            CancellationToken cancellationToken = default) =>
            _inner.PurgeTerminalAsync(terminalRetention, maximumCount, cancellationToken);
    }
}
