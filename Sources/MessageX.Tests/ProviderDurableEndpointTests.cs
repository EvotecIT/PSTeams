using System.Security.Cryptography;
using System.Text;
using MessageX.Discord;
using MessageX.Discord.Hosting.AspNetCore;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Persistence.DbaClientX;
using MessageX.Slack;
using MessageX.Slack.Hosting.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace MessageX.Tests;

public sealed class ProviderDurableEndpointTests {
    private const string SlackSecret = "durable-endpoint-secret";
    private const string SlackTimestamp = "1787418600";
    private const string DiscordTimestamp = "1787420400";
    private static readonly DateTimeOffset SlackNow = DateTimeOffset.FromUnixTimeSeconds(1787418600);
    private static readonly DateTimeOffset DiscordNow = DateTimeOffset.FromUnixTimeSeconds(1787420400);
    private static readonly Ed25519PrivateKeyParameters DiscordPrivateKey = new(
        Enumerable.Range(1, 32).Select(value => (byte)value).ToArray(),
        0);
    private static readonly string DiscordPublicKey = Convert.ToHexString(
        DiscordPrivateKey.GeneratePublicKey().GetEncoded());

    [Fact]
    public async Task SlackEndpointCommitsBeforeAckAndDispatchesOnceAfterRestart() {
        using var database = new TemporaryDatabase();
        var acceptanceGate = new AcceptanceGate();
        const string payload = """
            {"type":"block_actions","api_app_id":"A1","team":{"id":"T1"},"user":{"id":"U1"},"channel":{"id":"C1"},
             "trigger_id":"trigger-secret","response_url":"https://hooks.slack.com/actions/secret",
             "actions":[{"type":"button","action_id":"approve","value":"yes"}]}
            """;
        var body = "payload=" + Uri.EscapeDataString(payload);

        using (var accepting = SlackServices(database.Path, acceptanceGate).BuildServiceProvider()) {
            var handler = accepting.GetRequiredService<SlackHttpEndpointHandler>();
            var first = SlackContext(body);
            var duplicate = SlackContext(body);
            first.Response.StatusCode = StatusCodes.Status418ImATeapot;
            var acceptingFirst = handler.HandleInteractionsAsync(
                first,
                new SlackEndpointConfiguration("workspace-a", SlackSecret, "A1", "T1"),
                TestContext.Current.CancellationToken);
            await acceptanceGate.Entered.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.Equal(StatusCodes.Status418ImATeapot, first.Response.StatusCode);
            Assert.False(acceptingFirst.IsCompleted);
            acceptanceGate.Release();
            await acceptingFirst.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            await handler.HandleInteractionsAsync(
                duplicate,
                new SlackEndpointConfiguration("workspace-a", SlackSecret, "A1", "T1"),
                TestContext.Current.CancellationToken);

            Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);
            Assert.Equal(StatusCodes.Status200OK, duplicate.Response.StatusCode);
            Assert.Equal(2, accepting.GetRequiredService<IMessageDurableIngressHealth>()
                .GetHealthSnapshot().Accepted);
        }

        using var restarted = SlackServices(database.Path).BuildServiceProvider();
        var calls = 0;
        var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        restarted.GetRequiredService<MessageRouter>().OnAction<SlackInteractionEvent>("approve", (context, _) => {
            Interlocked.Increment(ref calls);
            Assert.Equal("yes", context.Envelope.Payload.ProviderPayload?.Actions[0].Value);
            Assert.Null(context.Envelope.Payload.TransientContext.TriggerId);
            Assert.Null(context.Envelope.Payload.TransientContext.ResponseUrl);
            handled.TrySetResult(true);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var workers = await StartWorkersAsync(restarted);
        await handled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await WaitUntilCompletedAsync(restarted);
        await StopWorkersAsync(workers);

        Assert.Equal(1, Volatile.Read(ref calls));
        Assert.Equal(1, restarted.GetRequiredService<IMessageDurableIngressHealth>()
            .GetHealthSnapshot().Completed);
    }

    [Fact]
    public async Task DiscordEndpointCommitsBeforeDeferredAckAndRestoresWithoutToken() {
        using var database = new TemporaryDatabase();
        var acceptanceGate = new AcceptanceGate();
        const string json = """
            {"id":"100000000000000001","application_id":"100000000000000002","type":2,
             "token":"interaction-secret","guild_id":"100000000000000003","channel_id":"100000000000000004",
             "authorizing_integration_owners":{"0":"100000000000000003"},
             "member":{"user":{"id":"100000000000000005"}},
             "data":{"name":"status","type":1,"options":[{"name":"target","value":"server-1"}]}}
            """;

        using (var accepting = DiscordServices(database.Path, acceptanceGate).BuildServiceProvider()) {
            var context = DiscordContext(json);
            context.Response.StatusCode = StatusCodes.Status418ImATeapot;
            var acceptingRequest = accepting.GetRequiredService<DiscordHttpEndpointHandler>().HandleAsync(
                context,
                new DiscordEndpointConfiguration("application-a", DiscordPublicKey,
                    "100000000000000002", "100000000000000003"),
                TestContext.Current.CancellationToken);
            await acceptanceGate.Entered.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.Equal(StatusCodes.Status418ImATeapot, context.Response.StatusCode);
            Assert.Empty(ResponseBody(context));
            Assert.False(acceptingRequest.IsCompleted);
            acceptanceGate.Release();
            await acceptingRequest.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.Contains("\"type\":5", Encoding.UTF8.GetString(ResponseBody(context)), StringComparison.Ordinal);
        }

        using var restarted = DiscordServices(database.Path).BuildServiceProvider();
        var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        restarted.GetRequiredService<MessageRouter>().OnCommand<DiscordInboundInteraction>("status", "1", (context, _) => {
            Assert.Equal("server-1", context.Envelope.Payload.Data
                .GetProperty("options")[0].GetProperty("value").GetString());
            Assert.False(context.Envelope.Payload.TransientContext.CanFollowUp);
            Assert.Null(context.Envelope.Payload.TransientContext.Token);
            handled.TrySetResult(true);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var workers = await StartWorkersAsync(restarted);
        await handled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await WaitUntilCompletedAsync(restarted);
        await StopWorkersAsync(workers);

        Assert.Equal(1, restarted.GetRequiredService<IMessageDurableIngressHealth>()
            .GetHealthSnapshot().Completed);
    }

    private static ServiceCollection SlackServices(string databasePath, AcceptanceGate? acceptanceGate = null) {
        var services = DurableServices(databasePath, SlackNow, acceptanceGate);
        services.AddMessageXSlackAspNetCore();
        return services;
    }

    private static ServiceCollection DiscordServices(string databasePath, AcceptanceGate? acceptanceGate = null) {
        var services = DurableServices(databasePath, DiscordNow, acceptanceGate);
        services.AddMessageXDiscordAspNetCore();
        return services;
    }

    private static ServiceCollection DurableServices(
        string databasePath,
        DateTimeOffset now,
        AcceptanceGate? acceptanceGate) {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(now));
        services.AddSingleton<IMessageDurableStore>(_ => new BlockingDurableStore(
            new SqliteMessageDurableStore(databasePath),
            acceptanceGate));
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableIngress(options => {
            options.PollInterval = TimeSpan.FromMilliseconds(10);
            options.RetryDelay = TimeSpan.Zero;
        });
        return services;
    }

    private static async Task<IHostedService[]> StartWorkersAsync(ServiceProvider provider) {
        var workers = provider.GetServices<IHostedService>().ToArray();
        foreach (var worker in workers) {
            await worker.StartAsync(TestContext.Current.CancellationToken);
        }
        return workers;
    }

    private static async Task StopWorkersAsync(IHostedService[] workers) {
        for (var index = workers.Length - 1; index >= 0; index--) {
            await workers[index].StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private static async Task WaitUntilCompletedAsync(ServiceProvider provider) {
        var health = provider.GetRequiredService<IMessageDurableIngressHealth>();
        var timeout = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < timeout) {
            if (health.GetHealthSnapshot().Completed == 1) {
                return;
            }
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("Durable provider work did not complete.");
    }

    private static DefaultHttpContext SlackContext(string body) {
        var context = Context(body, "application/x-www-form-urlencoded; charset=utf-8");
        context.Request.Headers["X-Slack-Request-Timestamp"] = SlackTimestamp;
        context.Request.Headers["X-Slack-Signature"] = SlackSign(body);
        return context;
    }

    private static DefaultHttpContext DiscordContext(string body) {
        var context = Context(body, "application/json; charset=utf-8");
        context.Request.Headers["X-Signature-Timestamp"] = DiscordTimestamp;
        context.Request.Headers["X-Signature-Ed25519"] = DiscordSign(body);
        return context;
    }

    private static DefaultHttpContext Context(string body, string contentType) {
        var bytes = Encoding.UTF8.GetBytes(body);
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "durable-provider-test";
        context.Request.ContentType = contentType;
        context.Request.ContentLength = bytes.Length;
        context.Request.Body = new MemoryStream(bytes);
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static byte[] ResponseBody(DefaultHttpContext context) =>
        ((MemoryStream)context.Response.Body).ToArray();

    private static string SlackSign(string body) {
        var prefix = Encoding.UTF8.GetBytes($"v0:{SlackTimestamp}:");
        var bytes = Encoding.UTF8.GetBytes(body);
        var signed = new byte[prefix.Length + bytes.Length];
        Buffer.BlockCopy(prefix, 0, signed, 0, prefix.Length);
        Buffer.BlockCopy(bytes, 0, signed, prefix.Length, bytes.Length);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SlackSecret));
        return "v0=" + string.Concat(hmac.ComputeHash(signed).Select(value => value.ToString("x2")));
    }

    private static string DiscordSign(string body) {
        var signer = new Ed25519Signer();
        signer.Init(true, DiscordPrivateKey);
        var timestamp = Encoding.ASCII.GetBytes(DiscordTimestamp);
        var bytes = Encoding.UTF8.GetBytes(body);
        signer.BlockUpdate(timestamp, 0, timestamp.Length);
        signer.BlockUpdate(bytes, 0, bytes.Length);
        return Convert.ToHexString(signer.GenerateSignature());
    }

    private sealed class FixedTimeProvider : TimeProvider {
        private readonly DateTimeOffset _now;
        public FixedTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }

    private sealed class AcceptanceGate {
        private readonly TaskCompletionSource<bool> _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Entered => _entered.Task;

        public async Task WaitAsync(CancellationToken cancellationToken) {
            _entered.TrySetResult(true);
            await _released.Task.WaitAsync(cancellationToken);
        }

        public void Release() => _released.TrySetResult(true);
    }

    private sealed class BlockingDurableStore : IMessageDurableStore, IDisposable {
        private readonly SqliteMessageDurableStore _inner;
        private readonly AcceptanceGate? _acceptanceGate;

        public BlockingDurableStore(SqliteMessageDurableStore inner, AcceptanceGate? acceptanceGate) {
            _inner = inner;
            _acceptanceGate = acceptanceGate;
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default) =>
            _inner.InitializeAsync(cancellationToken);

        public async Task<MessageDurableAcceptance> AcceptInboxAsync(
            MessageDurableRecord record,
            CancellationToken cancellationToken = default) {
            if (_acceptanceGate is not null) {
                await _acceptanceGate.WaitAsync(cancellationToken);
            }
            return await _inner.AcceptInboxAsync(record, cancellationToken);
        }

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
            CancellationToken cancellationToken = default) =>
            _inner.RenewInboxLeaseAsync(recordId, leaseToken, leaseDuration, cancellationToken);

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
            _inner.FailInboxAsync(recordId, leaseToken, failureKind, retryDelay, maximumAttempts, cancellationToken);

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
            _inner.FailOutboxAsync(recordId, leaseToken, failureKind, retryDelay, maximumAttempts, cancellationToken);

        public void Dispose() => _inner.Dispose();
    }

    private sealed class TemporaryDatabase : IDisposable {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "messagex-provider-durable-" + Guid.NewGuid().ToString("N"));

        public TemporaryDatabase() {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "messagex.db");
        }

        public string Path { get; }

        public void Dispose() {
            if (Directory.Exists(_directory)) {
                Directory.Delete(_directory, recursive: true);
            }
        }
    }
}
