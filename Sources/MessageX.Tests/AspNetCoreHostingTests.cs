using System.Text;
using System.Text.Json;
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
}
