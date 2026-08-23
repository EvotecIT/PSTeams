using MessageX.Hosting;
using MessageX.Persistence.DbaClientX;

namespace MessageX.Tests;

public sealed class SqliteMessageDurableStoreTests {
    private static readonly DateTimeOffset BaseTime =
        new(2026, 8, 22, 19, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task InitializeAcceptAndClaimRoundTripSafePayloadAndRoute() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.InitializeAsync();
        var source = Record("installation-a", "event-1", new byte[] { 1, 2, 3 });

        var accepted = await store.AcceptInboxAsync(source);
        var duplicate = await store.AcceptInboxAsync(source);
        var leases = await store.ClaimInboxAsync("worker-a", 10, TimeSpan.FromMinutes(1), BaseTime);

        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, accepted.Status);
        Assert.Equal(MessageDurableAcceptanceStatus.AlreadyPending, duplicate.Status);
        Assert.Equal(accepted.RecordId, duplicate.RecordId);
        var lease = Assert.Single(leases);
        Assert.Equal(1, lease.AttemptCount);
        Assert.Equal(source.Provider, lease.Record.Provider);
        Assert.Equal(source.InstallationId, lease.Record.InstallationId);
        Assert.Equal(source.DeduplicationKey, lease.Record.DeduplicationKey);
        Assert.Equal(MessageRouteKind.Action, lease.Record.Route.Kind);
        Assert.Equal(MessageEventKind.ActionInvoked, lease.Record.Route.EventKind);
        Assert.Equal("approve", lease.Record.Route.Name);
        Assert.Equal(new byte[] { 1, 2, 3 }, lease.Record.CopyPayload());
    }

    [Fact]
    public async Task DeduplicationIsScopedToProviderAndTrustedInstallation() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();

        var first = await store.AcceptInboxAsync(Record("installation-a", "same-event"));
        var second = await store.AcceptInboxAsync(Record("installation-b", "same-event"));
        var third = await store.AcceptInboxAsync(Record(
            "installation-a", "same-event", provider: MessageProviders.Discord));

        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, first.Status);
        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, second.Status);
        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, third.Status);
        Assert.NotEqual(first.RecordId, second.RecordId);
        Assert.NotEqual(first.RecordId, third.RecordId);
        Assert.Equal(3, (await store.ClaimInboxAsync(
            "worker-a", 10, TimeSpan.FromMinutes(1), BaseTime)).Count);
    }

    [Fact]
    public async Task DeduplicationCoordinatesUseOrdinalBinaryIdentity() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();

        var first = await store.AcceptInboxAsync(Record("Installation-A", "Event-A"));
        var second = await store.AcceptInboxAsync(Record("installation-a", "Event-A"));
        var third = await store.AcceptInboxAsync(Record("Installation-A", "event-a"));

        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, first.Status);
        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, second.Status);
        Assert.Equal(MessageDurableAcceptanceStatus.Accepted, third.Status);
        Assert.Equal(3, (await store.ClaimInboxAsync(
            "worker-a", 10, TimeSpan.FromMinutes(1), BaseTime)).Count);
    }

    [Fact]
    public async Task ConcurrentWorkersCannotLeaseTheSameInboxRecord() {
        using var database = new TemporaryDatabase();
        using var firstStore = new TestStore(database.Path);
        using var secondStore = new TestStore(database.Path);
        await firstStore.InitializeAsync();
        for (var index = 0; index < 10; index++) {
            await firstStore.AcceptInboxAsync(Record("installation-a", $"event-{index}"));
        }

        var claims = await Task.WhenAll(
            firstStore.ClaimInboxAsync("worker-a", 10, TimeSpan.FromMinutes(1), BaseTime),
            secondStore.ClaimInboxAsync("worker-b", 10, TimeSpan.FromMinutes(1), BaseTime));
        var leases = claims.SelectMany(static value => value).ToArray();

        Assert.Equal(10, leases.Length);
        Assert.Equal(10, leases.Select(static lease => lease.RecordId).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task RetryDelayLeaseOwnershipAndAttemptLimitAreEnforced() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-1"));
        var first = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));

        Assert.False(await store.CompleteInboxAsync(first.RecordId, "wrong-token", BaseTime));
        Assert.Equal(MessageDurableFailureStatus.LeaseLost, (await store.FailInboxAsync(
            first.RecordId,
            "wrong-token",
            MessageDurableFailureKind.Transient,
            BaseTime,
            TimeSpan.FromMinutes(5),
            2)).Status);
        Assert.Equal(MessageDurableFailureStatus.RetryScheduled, (await store.FailInboxAsync(
            first.RecordId,
            first.LeaseToken,
            MessageDurableFailureKind.Handler,
            BaseTime,
            TimeSpan.FromMinutes(5),
            2)).Status);
        Assert.Empty(await store.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(4)));

        var second = Assert.Single(await store.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(5)));
        Assert.Equal(2, second.AttemptCount);
        Assert.Equal(MessageDurableFailureStatus.DeadLettered, (await store.FailInboxAsync(
            second.RecordId,
            second.LeaseToken,
            MessageDurableFailureKind.Transient,
            BaseTime.AddMinutes(5),
            TimeSpan.Zero,
            2)).Status);
        Assert.Equal(MessageDurableAcceptanceStatus.DeadLettered,
            (await store.AcceptInboxAsync(Record("installation-a", "event-1"))).Status);
    }

    [Fact]
    public async Task ExpiredLeaseCanBeRecoveredAfterStoreRestart() {
        using var database = new TemporaryDatabase();
        string recordId;
        string firstToken;
        using (var firstStore = new TestStore(database.Path)) {
            await firstStore.InitializeAsync();
            await firstStore.AcceptInboxAsync(Record("installation-a", "event-1"));
            var firstLease = Assert.Single(await firstStore.ClaimInboxAsync(
                "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));
            recordId = firstLease.RecordId;
            firstToken = firstLease.LeaseToken;
        }

        using var restarted = new TestStore(database.Path);
        await restarted.InitializeAsync();
        Assert.Empty(await restarted.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddSeconds(59)));
        var recovered = Assert.Single(await restarted.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(1)));

        Assert.Equal(recordId, recovered.RecordId);
        Assert.NotEqual(firstToken, recovered.LeaseToken);
        Assert.Equal(2, recovered.AttemptCount);
        Assert.False(await restarted.CompleteInboxAsync(recordId, firstToken, BaseTime.AddMinutes(1)));
        Assert.True(await restarted.CompleteInboxAsync(
            recordId, recovered.LeaseToken, BaseTime.AddMinutes(1)));
        Assert.Equal(MessageDurableAcceptanceStatus.AlreadyCompleted,
            (await restarted.AcceptInboxAsync(Record("installation-a", "event-1"))).Status);
    }

    [Fact]
    public async Task InboxRenewalRequiresCurrentTokenAndCannotResurrectExpiredLease() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-1"));
        var lease = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));

        Assert.Null(await store.RenewInboxLeaseAsync(
            lease.RecordId, "wrong-token", TimeSpan.FromMinutes(2), BaseTime.AddSeconds(30)));
        Assert.NotNull(await store.RenewInboxLeaseAsync(
            lease.RecordId, lease.LeaseToken, TimeSpan.FromMinutes(2), BaseTime.AddSeconds(30)));
        Assert.Empty(await store.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(1)));
        Assert.Null(await store.RenewInboxLeaseAsync(
            lease.RecordId, lease.LeaseToken, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(2).AddSeconds(30)));
        var recovered = Assert.Single(await store.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(2).AddSeconds(30)));
        Assert.NotEqual(lease.LeaseToken, recovered.LeaseToken);
    }

    [Fact]
    public async Task ExpiredInboxLeaseCannotCompleteOrFailBeforeAnotherWorkerReclaimsIt() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-1"));
        var lease = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));
        var expiredAt = BaseTime.AddMinutes(1);

        Assert.False(await store.CompleteInboxAsync(
            lease.RecordId, lease.LeaseToken, expiredAt));
        Assert.Equal(MessageDurableFailureStatus.LeaseLost, (await store.FailInboxAsync(
            lease.RecordId,
            lease.LeaseToken,
            MessageDurableFailureKind.Handler,
            expiredAt,
            TimeSpan.Zero,
            3)).Status);
        Assert.Single(await store.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), expiredAt));
    }

    [Fact]
    public async Task InboxCompletionCommitsOutboxAndOutboxLifecycle() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-1"));
        var inbox = Assert.Single(await store.ClaimInboxAsync(
            "handler-a", 1, TimeSpan.FromMinutes(1), BaseTime));
        var outbox = new MessageOutboxRecord(
            MessageProviders.Slack,
            "installation-a",
            "reply-event-1",
            "send-message",
            "slack.send.v1",
            new byte[] { 4, 5, 6 },
            BaseTime.AddMinutes(1));

        Assert.True(await store.CompleteInboxAsync(
            inbox.RecordId, inbox.LeaseToken, BaseTime, new[] { outbox }));
        Assert.Empty(await store.ClaimOutboxAsync(
            "sender-a", 1, TimeSpan.FromMinutes(1), BaseTime));
        var delivery = Assert.Single(await store.ClaimOutboxAsync(
            "sender-a", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(1)));
        Assert.Equal(1, delivery.AttemptCount);
        Assert.Equal("send-message", delivery.Record.Operation);
        Assert.Equal(new byte[] { 4, 5, 6 }, delivery.Record.CopyPayload());
        Assert.False(await store.CompleteOutboxAsync(
            delivery.RecordId, "wrong-token", BaseTime.AddMinutes(1)));
        Assert.True(await store.CompleteOutboxAsync(
            delivery.RecordId, delivery.LeaseToken, BaseTime.AddMinutes(1)));
        Assert.Empty(await store.ClaimOutboxAsync(
            "sender-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddMinutes(2)));
    }

    [Fact]
    public async Task PermanentOutboxFailureIsDeadLetteredWithoutPersistingFailureText() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-1"));
        var inbox = Assert.Single(await store.ClaimInboxAsync(
            "handler-a", 1, TimeSpan.FromMinutes(1), BaseTime));
        await store.CompleteInboxAsync(
            inbox.RecordId,
            inbox.LeaseToken,
            BaseTime,
            new[] {
                new MessageOutboxRecord(
                    MessageProviders.Discord,
                    "installation-a",
                    "send-1",
                    "send-message",
                    "discord.send.v1",
                    Array.Empty<byte>(),
                    BaseTime)
            });
        var delivery = Assert.Single(await store.ClaimOutboxAsync(
            "sender-a", 1, TimeSpan.FromMinutes(1), BaseTime));

        var failed = await store.FailOutboxAsync(
            delivery.RecordId,
            delivery.LeaseToken,
            MessageDurableFailureKind.Permanent,
            BaseTime,
            TimeSpan.Zero,
            5);

        Assert.Equal(MessageDurableFailureStatus.DeadLettered, failed.Status);
        Assert.Empty(await store.ClaimOutboxAsync(
            "sender-b", 1, TimeSpan.FromMinutes(1), BaseTime.AddDays(1)));
        var schema = System.Text.Encoding.UTF8.GetString(await File.ReadAllBytesAsync(
            database.Path,
            TestContext.Current.CancellationToken));
        Assert.DoesNotContain("exception-message", schema, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DurableCommandQualifierRoundTripsThroughStorage() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(new MessageDurableRecord(
            MessageProviders.Discord,
            "application-a",
            "interaction-1",
            MessageRoute.ForCommand("inspect", "2"),
            BaseTime,
            "discord.interaction.v1",
            Array.Empty<byte>()));

        var lease = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));

        Assert.Equal("inspect", lease.Record.Route.Name);
        Assert.Equal("2", lease.Record.Route.Qualifier);
    }

    [Fact]
    public async Task StoreClockGuardsExpiryAndRenewalForInbox() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-renew"));
        var renewedInbox = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));
        var renewal = await store.RenewInboxLeaseAsync(
            renewedInbox.RecordId,
            renewedInbox.LeaseToken,
            TimeSpan.FromMinutes(1),
            BaseTime.AddSeconds(30));

        Assert.NotNull(renewal);
        Assert.Equal(BaseTime.AddSeconds(90), renewal.LeaseExpiresAt);
        Assert.True(await store.CompleteInboxAsync(
            renewedInbox.RecordId,
            renewedInbox.LeaseToken,
            BaseTime.AddSeconds(61)));

        await store.AcceptInboxAsync(Record("installation-a", "event-expired"));
        var expiredInbox = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime.AddHours(1)));
        var expiredAt = BaseTime.AddHours(1).AddMinutes(1);

        Assert.Null(await store.RenewInboxLeaseAsync(
            expiredInbox.RecordId,
            expiredInbox.LeaseToken,
            TimeSpan.FromMinutes(1),
            expiredAt));
        Assert.False(await store.CompleteInboxAsync(
            expiredInbox.RecordId,
            expiredInbox.LeaseToken,
            expiredAt));
        Assert.Equal(MessageDurableFailureStatus.LeaseLost, (await store.FailInboxAsync(
            expiredInbox.RecordId,
            expiredInbox.LeaseToken,
            MessageDurableFailureKind.Transient,
            expiredAt,
            TimeSpan.Zero,
            3)).Status);
        Assert.Single(await store.ClaimInboxAsync(
            "worker-b", 1, TimeSpan.FromMinutes(1), expiredAt));
    }

    [Fact]
    public async Task StoreClockGuardsExpiryAndRenewalForOutbox() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-outbox"));
        var inbox = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));
        await store.CompleteInboxAsync(
            inbox.RecordId,
            inbox.LeaseToken,
            BaseTime,
            new[] {
                new MessageOutboxRecord(
                    MessageProviders.Discord,
                    "installation-a",
                    "send-renew",
                    "send-message",
                    "discord.send.v1",
                    Array.Empty<byte>(),
                    BaseTime),
                new MessageOutboxRecord(
                    MessageProviders.Discord,
                    "installation-a",
                    "send-expire",
                    "send-message",
                    "discord.send.v1",
                    Array.Empty<byte>(),
                    BaseTime)
            });
        var deliveries = await store.ClaimOutboxAsync(
            "sender-a", 2, TimeSpan.FromMinutes(1), BaseTime);
        var renewedOutbox = Assert.Single(deliveries, lease =>
            lease.Record.DeduplicationKey == "send-renew");
        var expiredOutbox = Assert.Single(deliveries, lease =>
            lease.Record.DeduplicationKey == "send-expire");
        var renewal = await store.RenewOutboxLeaseAsync(
            renewedOutbox.RecordId,
            renewedOutbox.LeaseToken,
            TimeSpan.FromMinutes(1),
            BaseTime.AddSeconds(30));

        Assert.NotNull(renewal);
        Assert.True(await store.CompleteOutboxAsync(
            renewedOutbox.RecordId,
            renewedOutbox.LeaseToken,
            BaseTime.AddSeconds(61)));

        var expiredAt = BaseTime.AddMinutes(1);
        Assert.Null(await store.RenewOutboxLeaseAsync(
            expiredOutbox.RecordId,
            expiredOutbox.LeaseToken,
            TimeSpan.FromMinutes(1),
            expiredAt));
        Assert.False(await store.CompleteOutboxAsync(
            expiredOutbox.RecordId,
            expiredOutbox.LeaseToken,
            expiredAt));
        Assert.Equal(MessageDurableFailureStatus.LeaseLost, (await store.FailOutboxAsync(
            expiredOutbox.RecordId,
            expiredOutbox.LeaseToken,
            MessageDurableFailureKind.Transient,
            expiredAt,
            TimeSpan.Zero,
            3)).Status);
        Assert.Single(await store.ClaimOutboxAsync(
            "sender-b", 1, TimeSpan.FromMinutes(1), expiredAt));
    }

    [Theory]
    [InlineData(":memory:")]
    [InlineData("file::memory:?cache=shared")]
    [InlineData("file:messagex?mode=memory&cache=shared")]
    [InlineData("file:%3Amemory%3A?cache=shared")]
    [InlineData("file:messagex?mode%3Dmemory&cache=shared")]
    public void InMemoryDatabasePathsAreRejected(string databasePath) {
        Assert.Throws<ArgumentException>(() => new SqliteMessageDurableStore(databasePath));
    }

    [Fact]
    public void RelativeFileUriIsResolvedOnceAndRetainsItsQuery() {
        using var store = new SqliteMessageDurableStore("file:messagex-relative.db?mode=rwc&cache=private");
        var field = typeof(SqliteMessageDurableStore).GetField(
            "_databasePath",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        var normalized = Assert.IsType<string>(field?.GetValue(store));

        Assert.Equal(
            "file:" + Path.GetFullPath("messagex-relative.db") + "?mode=rwc&cache=private",
            normalized);
    }

    [Fact]
    public async Task UndefinedFailureKindsAreRejected() {
        using var database = new TemporaryDatabase();
        using var store = new TestStore(database.Path);
        await store.InitializeAsync();
        await store.AcceptInboxAsync(Record("installation-a", "event-invalid-failure"));
        var lease = Assert.Single(await store.ClaimInboxAsync(
            "worker-a", 1, TimeSpan.FromMinutes(1), BaseTime));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => store.FailInboxAsync(
            lease.RecordId,
            lease.LeaseToken,
            (MessageDurableFailureKind)99,
            BaseTime,
            TimeSpan.Zero,
            3));
    }

    private static MessageDurableRecord Record(
        string installationId,
        string deduplicationKey,
        byte[]? payload = null,
        string provider = MessageProviders.Slack) => new(
        provider,
        installationId,
        deduplicationKey,
        MessageRoute.ForAction("approve"),
        BaseTime,
        "slack.action.v1",
        payload ?? Array.Empty<byte>());

    private sealed class TestStore : IDisposable {
        private static readonly string[] SupportedPayloadTypes = {
            "slack.action.v1",
            "discord.interaction.v1",
            "slack.send.v1",
            "discord.send.v1"
        };
        private readonly SqliteMessageDurableStore _store;
        private readonly MutableTimeProvider _timeProvider = new();

        public TestStore(string databasePath) =>
            _store = SqliteMessageDurableStore.CreateWithTimeProvider(databasePath, _timeProvider);

        public Task InitializeAsync() =>
            _store.InitializeAsync(TestContext.Current.CancellationToken);

        public Task<MessageDurableAcceptance> AcceptInboxAsync(MessageDurableRecord record) =>
            _store.AcceptInboxAsync(record, TestContext.Current.CancellationToken);

        public Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            DateTimeOffset now) {
            _timeProvider.Set(now);
            return _store.RenewInboxLeaseAsync(
                recordId,
                leaseToken,
                leaseDuration,
                TestContext.Current.CancellationToken);
        }

        public Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            DateTimeOffset now) {
            _timeProvider.Set(now);
            return _store.ClaimInboxAsync(
                ownerId,
                maximumCount,
                leaseDuration,
                SupportedPayloadTypes,
                TestContext.Current.CancellationToken);
        }

        public Task<bool> CompleteInboxAsync(
            string recordId,
            string leaseToken,
            DateTimeOffset completedAt,
            IReadOnlyList<MessageOutboxRecord>? outbox = null) {
            _timeProvider.Set(completedAt);
            return _store.CompleteInboxAsync(
                recordId,
                leaseToken,
                outbox is null ? null : new MessageOutboxBatch(outbox),
                TestContext.Current.CancellationToken);
        }

        public Task<MessageDurableFailureResult> FailInboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            DateTimeOffset now,
            TimeSpan retryDelay,
            int maximumAttempts) {
            _timeProvider.Set(now);
            return _store.FailInboxAsync(
                recordId,
                leaseToken,
                failureKind,
                retryDelay,
                maximumAttempts,
                TestContext.Current.CancellationToken);
        }

        public Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
            string ownerId,
            int maximumCount,
            TimeSpan leaseDuration,
            DateTimeOffset now) {
            _timeProvider.Set(now);
            return _store.ClaimOutboxAsync(
                ownerId,
                maximumCount,
                leaseDuration,
                SupportedPayloadTypes,
                TestContext.Current.CancellationToken);
        }

        public Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
            string recordId,
            string leaseToken,
            TimeSpan leaseDuration,
            DateTimeOffset now) {
            _timeProvider.Set(now);
            return _store.RenewOutboxLeaseAsync(
                recordId,
                leaseToken,
                leaseDuration,
                TestContext.Current.CancellationToken);
        }

        public Task<bool> CompleteOutboxAsync(
            string recordId,
            string leaseToken,
            DateTimeOffset completedAt) {
            _timeProvider.Set(completedAt);
            return _store.CompleteOutboxAsync(
                recordId,
                leaseToken,
                TestContext.Current.CancellationToken);
        }

        public Task<MessageDurableFailureResult> FailOutboxAsync(
            string recordId,
            string leaseToken,
            MessageDurableFailureKind failureKind,
            DateTimeOffset now,
            TimeSpan retryDelay,
            int maximumAttempts) {
            _timeProvider.Set(now);
            return _store.FailOutboxAsync(
                recordId,
                leaseToken,
                failureKind,
                retryDelay,
                maximumAttempts,
                TestContext.Current.CancellationToken);
        }

        public void Dispose() => _store.Dispose();
    }

    private sealed class MutableTimeProvider : TimeProvider {
        private DateTimeOffset _utcNow = BaseTime;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Set(DateTimeOffset utcNow) => _utcNow = utcNow.ToUniversalTime();
    }

    private sealed class TemporaryDatabase : IDisposable {
        public TemporaryDatabase() => Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"messagex-{Guid.NewGuid():N}.db");

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
