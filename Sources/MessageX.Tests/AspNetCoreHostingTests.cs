using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageX.Tests;

public sealed class AspNetCoreHostingTests {
    private static readonly DateTimeOffset FixedNow =
        new(2026, 8, 22, 18, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task RequestReaderPreservesExactBytesAndTrustedRouteCoordinates() {
        var body = new byte[] { 0, 1, 2, 255 };
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/octet-stream";
        context.Request.ContentLength = body.Length;
        context.Request.Body = new MemoryStream(body);
        var reader = Reader(maximumBodyBytes: body.Length);

        var request = await reader.ReadAsync(
            context.Request,
            " installation-1 ",
            TestContext.Current.CancellationToken);

        Assert.Equal(body, request.CopyBody());
        Assert.Equal("installation-1", request.InstallationId);
        Assert.Equal("application/octet-stream", request.ContentType);
        Assert.Equal(FixedNow, request.ReceivedAt);
    }

    [Fact]
    public async Task RequestReaderRejectsDeclaredAndStreamingBodiesOverTheLimit() {
        var declared = new DefaultHttpContext();
        declared.Request.ContentLength = 5;
        declared.Request.Body = new MemoryStream(new byte[4]);
        var streamed = new DefaultHttpContext();
        streamed.Request.Body = new MemoryStream(new byte[5]);
        var reader = Reader(maximumBodyBytes: 4);

        var declaredError = await Assert.ThrowsAsync<MessageInboundBodyTooLargeException>(() =>
            reader.ReadAsync(declared.Request, "installation-1", TestContext.Current.CancellationToken));
        var streamedError = await Assert.ThrowsAsync<MessageInboundBodyTooLargeException>(() =>
            reader.ReadAsync(streamed.Request, "installation-1", TestContext.Current.CancellationToken));

        Assert.Equal(4, declaredError.MaximumBodyBytes);
        Assert.Equal(4, streamedError.MaximumBodyBytes);
    }

    [Fact]
    public async Task AcknowledgementWriterPreservesExactStatusContentTypeAndBody() {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var acknowledgement = new MessageAcknowledgement(
            StatusCodes.Status200OK,
            "application/json",
            Encoding.UTF8.GetBytes("{\"type\":1}"));

        await new MessageAcknowledgementWriter().WriteAsync(
            context.Response,
            acknowledgement,
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal(10, context.Response.ContentLength);
        Assert.Equal("{\"type\":1}", Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray()));

        context.Response.ContentType = "text/plain";
        await new MessageAcknowledgementWriter().WriteAsync(
            context.Response,
            MessageAcknowledgement.Empty(StatusCodes.Status204NoContent),
            TestContext.Current.CancellationToken);
        Assert.Null(context.Response.ContentType);
    }

    [Fact]
    public void QueueRejectsNonDispatchResultsAndFailsClosedWhenFull() {
        using var provider = Services(capacity: 1).BuildServiceProvider();
        var queue = provider.GetRequiredService<IMessageIngressQueue>();

        Assert.Equal(MessageIngressEnqueueStatus.Accepted, queue.TryEnqueue(Dispatch("first")));
        Assert.Equal(MessageIngressEnqueueStatus.Full, queue.TryEnqueue(Dispatch("second")));
        Assert.Throws<ArgumentException>(() => queue.TryEnqueue(
            MessageReceiveResult<TestPayload>.Acknowledge(MessageAcknowledgement.Empty(200))));

        var health = queue.GetHealthSnapshot();
        Assert.Equal(1, health.Capacity);
        Assert.Equal(1, health.Queued);
        Assert.Equal(1, health.Accepted);
        Assert.Equal(0, health.Completed);
        Assert.Equal(0, health.Failed);
    }

    [Fact]
    public async Task WorkerIsolatesHandlerFailuresAndContinuesDispatching() {
        using var provider = Services(capacity: 4).BuildServiceProvider();
        var router = provider.GetRequiredService<MessageRouter>();
        var queue = provider.GetRequiredService<IMessageIngressQueue>();
        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        router.OnCommand<TestPayload>("status", (context, _) => {
            if (context.Envelope.Payload.Text == "fail") {
                throw new InvalidOperationException("secret provider payload");
            }
            completed.TrySetResult(true);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var worker = provider.GetServices<IHostedService>().Single();
        await worker.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(MessageIngressEnqueueStatus.Accepted, queue.TryEnqueue(Dispatch("fail")));
        Assert.Equal(MessageIngressEnqueueStatus.Accepted, queue.TryEnqueue(Dispatch("complete")));
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await WaitForHealthAsync(queue, expectedCompleted: 1, expectedFailed: 1);

        var health = queue.GetHealthSnapshot();
        Assert.Equal(2, health.Accepted);
        Assert.Equal(1, health.Completed);
        Assert.Equal(1, health.Failed);
        Assert.NotNull(health.LastCompletedAt);
        Assert.NotNull(health.LastFailureAt);
        Assert.DoesNotContain("secret provider payload", JsonSerializer.Serialize(health), StringComparison.Ordinal);
        await worker.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal(MessageIngressEnqueueStatus.Stopping, queue.TryEnqueue(Dispatch("late")));
        Assert.True(queue.GetHealthSnapshot().IsStopping);
    }

    [Fact]
    public async Task WorkerPreservesTrustedInstallationIdentityAcrossOneSharedQueue() {
        using var provider = Services(capacity: 4).BuildServiceProvider();
        var router = provider.GetRequiredService<MessageRouter>();
        var queue = provider.GetRequiredService<IMessageIngressQueue>();
        var installations = new HashSet<string>(StringComparer.Ordinal);
        var sync = new object();
        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        router.OnCommand<TestPayload>("status", (context, _) => {
            lock (sync) {
                installations.Add(context.Envelope.InstallationId);
                if (installations.Count == 2) {
                    completed.TrySetResult(true);
                }
            }
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var worker = provider.GetServices<IHostedService>().Single();
        await worker.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(MessageIngressEnqueueStatus.Accepted, queue.TryEnqueue(Dispatch("first", "tenant-a")));
        Assert.Equal(MessageIngressEnqueueStatus.Accepted, queue.TryEnqueue(Dispatch("second", "tenant-b")));
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "tenant-a", "tenant-b" }, installations.OrderBy(value => value));
    }

    [Fact]
    public async Task WorkerConsumesTheRegisteredQueueAbstraction() {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(FixedNow));
        services.AddSingleton<IMessageIngressQueue, TestIngressQueue>();
        services.AddMessageXHostingAspNetCore();
        using var provider = services.BuildServiceProvider();
        var queue = Assert.IsType<TestIngressQueue>(provider.GetRequiredService<IMessageIngressQueue>());
        var router = provider.GetRequiredService<MessageRouter>();
        var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        router.OnCommand<TestPayload>("status", (_, _) => {
            handled.TrySetResult(true);
            return Task.FromResult(MessageHandlerResult.Completed());
        });
        var worker = provider.GetServices<IHostedService>().Single();

        await worker.StartAsync(TestContext.Current.CancellationToken);
        Assert.Equal(MessageIngressEnqueueStatus.Accepted, queue.TryEnqueue(Dispatch("custom")));
        await handled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, queue.CompletedCount);
    }

    [Fact]
    public async Task GracefulStopDrainsAlreadyAcceptedWorkWithinTheShutdownDeadline() {
        using var provider = Services(capacity: 4).BuildServiceProvider();
        var queue = provider.GetRequiredService<IMessageIngressQueue>();
        var router = provider.GetRequiredService<MessageRouter>();
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handled = 0;
        router.OnCommand<TestPayload>("status", async (context, cancellationToken) => {
            if (context.Envelope.Payload.Text == "first") {
                firstStarted.TrySetResult(true);
                await releaseFirst.Task.WaitAsync(cancellationToken);
            }
            Interlocked.Increment(ref handled);
            return MessageHandlerResult.Completed();
        });
        var worker = provider.GetServices<IHostedService>().Single();
        await worker.StartAsync(TestContext.Current.CancellationToken);
        queue.TryEnqueue(Dispatch("first"));
        queue.TryEnqueue(Dispatch("second"));
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var stop = worker.StopAsync(deadline.Token);
        Assert.False(stop.IsCompleted);
        releaseFirst.TrySetResult(true);
        await stop;

        Assert.Equal(2, Volatile.Read(ref handled));
        Assert.Equal(2, queue.GetHealthSnapshot().Completed);
    }

    [Fact]
    public void DependencyInjectionValidatesBoundsAndDoesNotReplaceExistingRouter() {
        var existingRouter = new MessageRouter();
        var services = new ServiceCollection();
        services.AddSingleton(existingRouter);
        services.AddMessageXHostingAspNetCore(options => options.QueueCapacity = 0);
        using var provider = services.BuildServiceProvider();

        Assert.Same(existingRouter, provider.GetRequiredService<MessageRouter>());
        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value);
    }

    [Fact]
    public void DependencyInjectionRejectsReplayRetentionShorterThanProviderVerificationWindows() {
        var services = new ServiceCollection();
        services.AddMessageXHostingAspNetCore(options =>
            options.ReplayRetention = MessageXHostingAspNetCoreOptions.MinimumReplayRetention -
                TimeSpan.FromTicks(1));
        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value);

        Assert.Contains(nameof(MessageXHostingAspNetCoreOptions.ReplayRetention), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReplayCapacitySupportsHighVolumeHostsWithAnIndependentBound() {
        var services = new ServiceCollection();
        services.AddMessageXHostingAspNetCore(options => options.ReplayCapacity = 1_000_000);
        using var provider = services.BuildServiceProvider();

        Assert.Equal(
            1_000_000,
            provider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value.ReplayCapacity);

        var invalidServices = new ServiceCollection();
        invalidServices.AddMessageXHostingAspNetCore(options =>
            options.ReplayCapacity = MessageXHostingAspNetCoreOptions.MaximumReplayCapacity + 1);
        using var invalidProvider = invalidServices.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() =>
            invalidProvider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value);
    }

    [Fact]
    public void SynchronousDispatchCapacityIsValidatedIndependently() {
        var services = new ServiceCollection();
        services.AddMessageXHostingAspNetCore(options => options.SynchronousDispatchCapacity = 2);
        using var provider = services.BuildServiceProvider();

        Assert.Equal(
            2,
            provider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>()
                .Value.SynchronousDispatchCapacity);

        var invalidServices = new ServiceCollection();
        invalidServices.AddMessageXHostingAspNetCore(options => options.SynchronousDispatchCapacity = 0);
        using var invalidProvider = invalidServices.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() =>
            invalidProvider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value);
    }

    private static MessageInboundRequestReader Reader(int maximumBodyBytes) => new(
        Options.Create(new MessageXHostingAspNetCoreOptions {
            MaximumRequestBodyBytes = maximumBodyBytes
        }),
        new FixedTimeProvider(FixedNow));

    private static ServiceCollection Services(int capacity) {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(FixedNow));
        services.AddMessageXHostingAspNetCore(options => options.QueueCapacity = capacity);
        return services;
    }

    private static MessageReceiveResult<TestPayload> Dispatch(
        string text,
        string installationId = "installation-1") {
        var envelope = new MessageEventEnvelope<TestPayload>(
            MessageProviders.Slack,
            installationId,
            $"event-{text}",
            MessageEventKind.CommandInvoked,
            FixedNow,
            new TestPayload(text));
        return MessageReceiveResult<TestPayload>.Dispatch(
            MessageRoute.ForCommand("status"),
            envelope,
            MessageAcknowledgement.Empty(200));
    }

    private static async Task WaitForHealthAsync(
        IMessageIngressQueue queue,
        long expectedCompleted,
        long expectedFailed) {
        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < timeoutAt) {
            var health = queue.GetHealthSnapshot();
            if (health.Completed == expectedCompleted && health.Failed == expectedFailed) {
                return;
            }
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("The ingress worker did not reach the expected safe health state.");
    }

    private sealed record TestPayload(string Text);

    private sealed class FixedTimeProvider : TimeProvider {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    private sealed class TestIngressQueue : IMessageIngressQueue {
        private readonly Channel<IMessageIngressWorkItem> _channel = Channel.CreateUnbounded<IMessageIngressWorkItem>();
        private long _accepted;
        private long _completed;
        private long _failed;
        private int _stopping;

        public long CompletedCount => Interlocked.Read(ref _completed);

        public MessageIngressEnqueueStatus TryEnqueue<TProviderPayload>(MessageReceiveResult<TProviderPayload> result) {
            if (Volatile.Read(ref _stopping) != 0) {
                return MessageIngressEnqueueStatus.Stopping;
            }
            if (result.Route is null || result.Envelope is null) {
                throw new ArgumentException("A dispatch-ready result is required.", nameof(result));
            }
            if (!_channel.Writer.TryWrite(new TestWorkItem<TProviderPayload>(result.Route, result.Envelope))) {
                return MessageIngressEnqueueStatus.Full;
            }
            Interlocked.Increment(ref _accepted);
            return MessageIngressEnqueueStatus.Accepted;
        }

        public IAsyncEnumerable<IMessageIngressWorkItem> ReadAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);

        public void Completed(DateTimeOffset at) => Interlocked.Increment(ref _completed);

        public void Failed(DateTimeOffset at) => Interlocked.Increment(ref _failed);

        public void Complete() {
            Interlocked.Exchange(ref _stopping, 1);
            _channel.Writer.TryComplete();
        }

        public MessageIngressHealthSnapshot GetHealthSnapshot() => new(
            int.MaxValue,
            _channel.Reader.Count,
            Interlocked.Read(ref _accepted),
            Interlocked.Read(ref _completed),
            Interlocked.Read(ref _failed),
            Volatile.Read(ref _stopping) != 0,
            null,
            null);

        private sealed class TestWorkItem<TProviderPayload> : IMessageIngressWorkItem {
            private readonly MessageRoute _route;
            private readonly MessageEventEnvelope<TProviderPayload> _envelope;

            public TestWorkItem(MessageRoute route, MessageEventEnvelope<TProviderPayload> envelope) {
                _route = route;
                _envelope = envelope;
            }

            public Task<MessageDispatchResult> DispatchAsync(
                MessageRouter router,
                CancellationToken cancellationToken) =>
                router.DispatchAsync(_route, _envelope, cancellationToken);
        }
    }
}
