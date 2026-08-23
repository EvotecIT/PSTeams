using System.Text;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Persistence.DbaClientX;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageX.Tests;

public sealed class DurableLeaseFinalizationTests {
    private static readonly DateTimeOffset FixedNow =
        new(2026, 8, 23, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InboxLeaseRenewsUntilCompletionCommits() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var store = new FinalizationProbeStore(innerStore, TerminalKind.InboxCompletion);
        using var provider = Services(store).BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<LeasePayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.Completed()));

        await AcceptAsync(provider, "inbox-terminal");
        var workers = await StartWorkersAsync(provider);
        try {
            await store.TerminalEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            await store.RenewalAfterTerminal.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            store.ReleaseTerminal();
            await store.TerminalCompleted.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
        } finally {
            store.ReleaseTerminal();
            await StopWorkersAsync(workers);
        }
    }

    [Fact]
    public async Task OutboxLeaseRenewsUntilCompletionCommits() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var store = new FinalizationProbeStore(innerStore, TerminalKind.OutboxCompletion);
        var outbox = new ProbeOutboxHandler(throwAfterAttempt: false);
        using var provider = Services(store, outbox).BuildServiceProvider();
        RegisterOutboxProducer(provider);

        await AcceptAsync(provider, "outbox-complete-terminal");
        var workers = await StartWorkersAsync(provider);
        try {
            await store.TerminalEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            await store.RenewalAfterTerminal.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            store.ReleaseTerminal();
            await store.TerminalCompleted.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
        } finally {
            store.ReleaseTerminal();
            await StopWorkersAsync(workers);
        }
    }

    [Fact]
    public async Task OutboxLeaseRenewsUntilFailureTransitionCommits() {
        using var database = new TemporaryDatabase();
        using var innerStore = new SqliteMessageDurableStore(database.Path);
        var store = new FinalizationProbeStore(innerStore, TerminalKind.OutboxFailure);
        var outbox = new ProbeOutboxHandler(throwAfterAttempt: true);
        using var provider = Services(store, outbox).BuildServiceProvider();
        RegisterOutboxProducer(provider);

        await AcceptAsync(provider, "outbox-failure-terminal");
        var workers = await StartWorkersAsync(provider);
        try {
            await store.TerminalEntered.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            await store.RenewalAfterTerminal.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            store.ReleaseTerminal();
            await store.TerminalCompleted.Task.WaitAsync(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken);
            Assert.Equal(1, outbox.Attempts);
        } finally {
            store.ReleaseTerminal();
            await StopWorkersAsync(workers);
        }
    }

    private static ServiceCollection Services(
        IMessageDurableStore store,
        IMessageOutboxHandler? outbox = null) {
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(store);
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableIngress(options => {
            options.PollInterval = TimeSpan.FromMilliseconds(10);
            options.RetryDelay = TimeSpan.Zero;
            options.MaximumAttempts = 3;
            options.LeaseDuration = TimeSpan.FromSeconds(1);
        });
        services.AddMessageXDurableCodec<LeasePayload, LeasePayloadCodec>();
        if (outbox is not null) {
            services.AddSingleton(outbox);
        }
        return services;
    }

    private static void RegisterOutboxProducer(IServiceProvider provider) {
        provider.GetRequiredService<MessageRouter>().OnCommand<LeasePayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.CompletedWithOutbox(new MessageOutboxBatch([
                new MessageOutboxRecord(
                    MessageProviders.Slack,
                    "installation-a",
                    "lease-outbox",
                    "send",
                    ProbeOutboxHandler.PayloadTypeName,
                    Encoding.UTF8.GetBytes("deliver"),
                    FixedNow)
            ]))));
    }

    private static async Task AcceptAsync(IServiceProvider provider, string id) {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            context.Response,
            MessageReceiveResult<LeasePayload>.Dispatch(
                MessageRoute.ForCommand("status"),
                new MessageEventEnvelope<LeasePayload>(
                    MessageProviders.Slack,
                    "installation-a",
                    id,
                    MessageEventKind.CommandInvoked,
                    FixedNow,
                    new LeasePayload(id)),
                MessageAcknowledgement.Empty(StatusCodes.Status200OK)),
            TestContext.Current.CancellationToken);
    }

    private static async Task<IHostedService[]> StartWorkersAsync(IServiceProvider provider) {
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }
        return workers;
    }

    private static async Task StopWorkersAsync(IReadOnlyList<IHostedService> workers) {
        for (var index = workers.Count - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed record LeasePayload(string Value);

    private sealed class LeasePayloadCodec : IMessageDurableCodec<LeasePayload> {
        public string PayloadType => "test.lease-payload.v1";

        public MessageDurableRecord Encode(
            MessageRoute route,
            MessageEventEnvelope<LeasePayload> envelope) => new(
            envelope.Provider,
            envelope.InstallationId,
            envelope.DeduplicationKey,
            route,
            envelope.ReceivedAt,
            PayloadType,
            Encoding.UTF8.GetBytes(envelope.Payload.Value));

        public MessageEventEnvelope<LeasePayload> Decode(MessageDurableRecord record) => new(
            record.Provider,
            record.InstallationId,
            record.DeduplicationKey,
            record.Route.EventKind,
            record.ReceivedAt,
            new LeasePayload(Encoding.UTF8.GetString(record.CopyPayload())));
    }

    private sealed class ProbeOutboxHandler : IMessageOutboxHandler {
        public const string PayloadTypeName = "test.lease-outbox.v1";
        private readonly bool _throwAfterAttempt;
        private int _attempts;

        public ProbeOutboxHandler(bool throwAfterAttempt) => _throwAfterAttempt = throwAfterAttempt;

        public string PayloadType => PayloadTypeName;

        public int Attempts => Volatile.Read(ref _attempts);

        public Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _attempts);
            if (_throwAfterAttempt) {
                throw new InvalidOperationException("provider acceptance is unknown");
            }
            return Task.CompletedTask;
        }
    }

    private enum TerminalKind {
        InboxCompletion,
        OutboxCompletion,
        OutboxFailure
    }

    private sealed class FinalizationProbeStore : IMessageDurableStore {
        private readonly IMessageDurableStore _inner;
        private readonly TerminalKind _terminalKind;
        private readonly TaskCompletionSource<bool> _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _renewInboxCalls;
        private int _renewOutboxCalls;

        public FinalizationProbeStore(IMessageDurableStore inner, TerminalKind terminalKind) {
            _inner = inner;
            _terminalKind = terminalKind;
        }

        public TaskCompletionSource<bool> TerminalEntered { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> TerminalCompleted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> RenewalAfterTerminal { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int RenewInboxCalls => Volatile.Read(ref _renewInboxCalls);

        public int RenewOutboxCalls => Volatile.Read(ref _renewOutboxCalls);

        public int RenewalsAtTerminalEntry { get; private set; }

        public void ReleaseTerminal() => _release.TrySetResult(true);

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
            CancellationToken cancellationToken = default) =>
            _inner.ClaimInboxAsync(ownerId, maximumCount, leaseDuration, payloadTypes, cancellationToken);

        public Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) {
            var calls = Interlocked.Increment(ref _renewInboxCalls);
            if (_terminalKind == TerminalKind.InboxCompletion &&
                TerminalEntered.Task.IsCompleted &&
                calls > RenewalsAtTerminalEntry) {
                RenewalAfterTerminal.TrySetResult(true);
            }
            return _inner.RenewInboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);
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
            CancellationToken cancellationToken = default) =>
            ObserveTerminalAsync(
                TerminalKind.InboxCompletion,
                () => _inner.CompleteInboxAsync(recordId, leaseToken, outbox, cancellationToken),
                cancellationToken);

        public Task<MessageDurableFailureResult> FailInboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) =>
            _inner.FailInboxAsync(
                recordId, leaseToken, failureKind, retryDelay, maximumAttempts, cancellationToken);

        public Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            IReadOnlyCollection<string> payloadTypes,
            CancellationToken cancellationToken = default) =>
            _inner.ClaimOutboxAsync(ownerId, maximumCount, leaseDuration, payloadTypes, cancellationToken);

        public Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default) {
            var calls = Interlocked.Increment(ref _renewOutboxCalls);
            if (_terminalKind is TerminalKind.OutboxCompletion or TerminalKind.OutboxFailure &&
                TerminalEntered.Task.IsCompleted &&
                calls > RenewalsAtTerminalEntry) {
                RenewalAfterTerminal.TrySetResult(true);
            }
            return _inner.RenewOutboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);
        }

        public Task<bool> CompleteOutboxAsync(
            string recordId,
            string leaseToken,
            CancellationToken cancellationToken = default) =>
            ObserveTerminalAsync(
                TerminalKind.OutboxCompletion,
                () => _inner.CompleteOutboxAsync(recordId, leaseToken, cancellationToken),
                cancellationToken);

        public Task<MessageDurableFailureResult> FailOutboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            TimeSpan retryDelay,
            int maximumAttempts,
            CancellationToken cancellationToken = default) =>
            ObserveTerminalAsync(
                TerminalKind.OutboxFailure,
                () => _inner.FailOutboxAsync(
                    recordId, leaseToken, failureKind, retryDelay, maximumAttempts, cancellationToken),
                cancellationToken);

        public Task<int> PurgeTerminalAsync(
            DateTimeOffset completedBefore,
            int maximumCount,
            CancellationToken cancellationToken = default) =>
            _inner.PurgeTerminalAsync(completedBefore, maximumCount, cancellationToken);

        private async Task<T> ObserveTerminalAsync<T>(
            TerminalKind kind,
            Func<Task<T>> transition,
            CancellationToken cancellationToken) {
            if (_terminalKind != kind) {
                return await transition().ConfigureAwait(false);
            }
            RenewalsAtTerminalEntry = kind == TerminalKind.InboxCompletion
                ? RenewInboxCalls
                : RenewOutboxCalls;
            TerminalEntered.TrySetResult(true);
            await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            try {
                return await transition().ConfigureAwait(false);
            } finally {
                TerminalCompleted.TrySetResult(true);
            }
        }
    }

    private sealed class TemporaryDatabase : IDisposable {
        public TemporaryDatabase() => Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"messagex-lease-finalization-{Guid.NewGuid():N}.db");

        public string Path { get; }

        public void Dispose() => TemporaryPathCleanup.DeleteSqliteDatabase(Path);
    }
}
