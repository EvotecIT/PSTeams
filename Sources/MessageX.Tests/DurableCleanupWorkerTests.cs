using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageX.Tests;

public sealed class DurableCleanupWorkerTests {
    [Fact]
    public async Task CleanupDrainsFullTerminalBatchesBeforeWaitingForTheNextInterval() {
        var store = new PurgeSequenceStore(4, 2, 1);
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IMessageDurableStore>(store);
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableIngress(options => {
            options.CleanupBatchSize = 2;
            options.CleanupInterval = TimeSpan.FromMinutes(1);
        });
        using var provider = services.BuildServiceProvider();
        var worker = provider.GetServices<IHostedService>()
            .Single(service => service.GetType().Name == "MessageDurableCleanupWorker");

        await worker.StartAsync(TestContext.Current.CancellationToken);
        try {
            await store.Drained.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            Assert.Equal(3, store.PurgeCalls);
            Assert.All(store.BatchSizes, batchSize => Assert.Equal(2, batchSize));
            Assert.Single(store.RetentionBoundaries.Distinct());
        } finally {
            await worker.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed class PurgeSequenceStore(params int[] purgeResults) : IMessageDurableStore {
        private readonly Queue<int> _purgeResults = new(purgeResults);
        private readonly object _gate = new();

        public TaskCompletionSource<bool> Drained { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int PurgeCalls { get; private set; }

        public List<int> BatchSizes { get; } = new();

        public List<DateTimeOffset> RetentionBoundaries { get; } = new();

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> PurgeTerminalAsync(
            DateTimeOffset completedBefore,
            int maximumCount,
            CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate) {
                PurgeCalls++;
                BatchSizes.Add(maximumCount);
                RetentionBoundaries.Add(completedBefore);
                var result = _purgeResults.Dequeue();
                if (_purgeResults.Count == 0) {
                    Drained.TrySetResult(true);
                }
                return Task.FromResult(result);
            }
        }

        public Task<MessageDurableAcceptance> AcceptInboxAsync(
            MessageDurableRecord record,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> ReleaseInboxAsync(
            string recordId,
            string leaseToken,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> CompleteInboxAsync(
            string recordId,
            string leaseToken,
            MessageOutboxBatch? outbox = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<MessageDurableFailureResult> FailInboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> CompleteOutboxAsync(
            string recordId,
            string leaseToken,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<MessageDurableFailureResult> FailOutboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
