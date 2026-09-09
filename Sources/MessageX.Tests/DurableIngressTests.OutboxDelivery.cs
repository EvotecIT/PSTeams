using System.Text;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Persistence.DbaClientX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageX.Tests;

public sealed partial class DurableIngressTests {
    [Theory]
    [InlineData(120, 30, false)]
    [InlineData(10, 30, false)]
    [InlineData(120, 30, true)]
    public async Task OutboxPersistsProviderBackoffAndHoldsAmbiguousDelivery(int providerSeconds, int hostSeconds, bool ambiguous) {
        using var database = new TemporaryDatabase();
        var clock = new OutboxClock(FixedNow);
        using var store = SqliteMessageDurableStore.CreateWithTimeProvider(database.Path, clock);
        var services = Services(store, includeCodec: true, retryDelay: TimeSpan.FromSeconds(hostSeconds), timeProvider: clock);
        services.AddSingleton<IMessageOutboxHandler>(new BackoffOutboxHandler(TimeSpan.FromSeconds(providerSeconds), ambiguous));
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<MessageRouter>().OnCommand<TestPayload>("status", (_, _) =>
            Task.FromResult(MessageHandlerResult.CompletedWithOutbox(new MessageOutboxBatch(new[] {
                new MessageOutboxRecord(MessageProviders.Discord, "installation-a", "release-42", "announce",
                    "test.backoff.v1", Encoding.UTF8.GetBytes("release"), FixedNow)
            }))));
        await provider.GetRequiredService<MessageReceiveResultProcessor>().ProcessAsync(
            ResponseContext().Response, Dispatch("backoff"), TestContext.Current.CancellationToken);
        var workers = provider.GetServices<IHostedService>().ToArray();
        var health = provider.GetRequiredService<IMessageDurableOutboxHealth>();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }
        try {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            while (health.GetHealthSnapshot() is { Retried: 0, DeadLettered: 0 }) {
                await Task.Delay(10, timeout.Token);
            }
        } finally {
            foreach (var worker in workers.AsEnumerable().Reverse()) {
                await worker.StopAsync(TestContext.Current.CancellationToken);
            }
        }
        var snapshot = health.GetHealthSnapshot();
        Assert.Equal(1, snapshot.Claimed);
        Assert.Equal(ambiguous ? 0 : 1, snapshot.Retried);
        Assert.Equal(ambiguous ? 1 : 0, snapshot.DeadLettered);
        Assert.True(snapshot.IsStopping);
        Assert.Equal(FixedNow, snapshot.LastFailureAt);
        Assert.DoesNotContain("private-provider-detail", snapshot.ToString(), StringComparison.Ordinal);

        var delay = TimeSpan.FromSeconds(Math.Max(providerSeconds, hostSeconds));
        clock.Advance(delay - TimeSpan.FromTicks(1));
        Assert.Empty(await store.ClaimOutboxAsync("reader", 1, TimeSpan.FromMinutes(1), new[] { "test.backoff.v1" }, TestContext.Current.CancellationToken));
        clock.Advance(TimeSpan.FromTicks(1));
        var due = await store.ClaimOutboxAsync("reader", 1, TimeSpan.FromMinutes(1), new[] { "test.backoff.v1" }, TestContext.Current.CancellationToken);
        Assert.Equal(ambiguous ? 0 : 1, due.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(604801)]
    public void OutboxRejectsUnpersistableRetryDelays(int seconds) {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MessageOutboxDeliveryException(
            "rejected", MessageOutboxDeliveryOutcome.DefinitelyNotSent, retryAfter: TimeSpan.FromSeconds(seconds)));
    }

    private sealed class BackoffOutboxHandler(TimeSpan retryAfter, bool ambiguous) : IMessageOutboxHandler {
        public string PayloadType => "test.backoff.v1";
        public Task DeliverAsync(MessageOutboxRecord record, CancellationToken cancellationToken) =>
            Task.FromException(new MessageOutboxDeliveryException("private-provider-detail",
                ambiguous ? MessageOutboxDeliveryOutcome.Ambiguous : MessageOutboxDeliveryOutcome.DefinitelyNotSent,
                retryAfter: retryAfter));
    }

    private sealed class OutboxClock(DateTimeOffset now) : TimeProvider {
        private long _ticks = now.UtcTicks;
        public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref _ticks), TimeSpan.Zero);
        public void Advance(TimeSpan delay) => Interlocked.Add(ref _ticks, delay.Ticks);
    }
}
