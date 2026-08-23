using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MessageX.Discord;
using MessageX.Discord.Hosting.AspNetCore;
using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using MessageX.Slack.Hosting.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace MessageX.Tests;

public sealed class ProviderAspNetCoreEndpointTests {
    private const string SlackSecret = "test-signing-secret";
    private const string SlackTimestamp = "1787416800";
    private const string DiscordTimestamp = "1787420400";
    private static readonly DateTimeOffset SlackReceivedAt = DateTimeOffset.FromUnixTimeSeconds(1787416800);
    private static readonly DateTimeOffset DiscordReceivedAt = DateTimeOffset.FromUnixTimeSeconds(1787420400);
    private static readonly Ed25519PrivateKeyParameters DiscordPrivateKey = new(
        Enumerable.Range(1, 32).Select(value => (byte)value).ToArray(),
        0);
    private static readonly string DiscordPublicKey = Convert.ToHexString(
        DiscordPrivateKey.GeneratePublicKey().GetEncoded());

    [Fact]
    public async Task SlackUrlVerificationWritesImmediateExactChallengeWithoutEnqueueing() {
        using var provider = SlackServices(capacity: 1).BuildServiceProvider();
        const string json = "{\"type\":\"url_verification\",\"challenge\":\"challenge-123\"}";
        var context = SlackContext(json);

        await provider.GetRequiredService<SlackHttpEndpointHandler>().HandleEventsAsync(
            context,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        using var body = JsonDocument.Parse(ResponseBody(context));
        Assert.Equal("challenge-123", body.RootElement.GetProperty("challenge").GetString());
        Assert.Equal(0, provider.GetRequiredService<IMessageIngressQueue>().GetHealthSnapshot().Accepted);
    }

    [Fact]
    public async Task SlackDispatchFailsBeforeSuccessAcknowledgementWhenQueueIsFull() {
        using var provider = SlackServices(capacity: 1).BuildServiceProvider();
        var handler = provider.GetRequiredService<SlackHttpEndpointHandler>();
        var first = SlackContext(SlackEvent("Ev1"));
        var second = SlackContext(SlackEvent("Ev2"));

        await handler.HandleEventsAsync(
            first,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);
        await handler.HandleEventsAsync(
            second,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, second.Response.StatusCode);
        Assert.Equal("1", second.Response.Headers.RetryAfter.ToString());
        Assert.Equal(1, provider.GetRequiredService<IMessageIngressQueue>().GetHealthSnapshot().Accepted);
    }

    [Fact]
    public async Task SlackEndpointRejectsInvalidSignatureWithoutEnqueueing() {
        using var provider = SlackServices(capacity: 1).BuildServiceProvider();
        var context = SlackContext(SlackEvent("Ev1"));
        context.Request.Headers["X-Slack-Signature"] = "v0=" + new string('0', 64);

        await provider.GetRequiredService<SlackHttpEndpointHandler>().HandleEventsAsync(
            context,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(0, provider.GetRequiredService<IMessageIngressQueue>().GetHealthSnapshot().Accepted);
    }

    [Fact]
    public async Task DiscordPingWritesPongWithoutDispatch() {
        using var provider = DiscordServices(capacity: 1).BuildServiceProvider();
        const string json = "{\"type\":1}";
        var context = DiscordContext(json);

        await provider.GetRequiredService<DiscordHttpEndpointHandler>().HandleAsync(
            context,
            DiscordConfiguration(),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        using var body = JsonDocument.Parse(ResponseBody(context));
        Assert.Equal(1, body.RootElement.GetProperty("type").GetInt32());
        Assert.Equal(0, provider.GetRequiredService<IMessageIngressQueue>().GetHealthSnapshot().Accepted);
    }

    [Fact]
    public async Task AcceptedProviderRetriesAreAcknowledgedWithoutRedispatch() {
        using var provider = SlackServices(capacity: 2).BuildServiceProvider();
        var handler = provider.GetRequiredService<SlackHttpEndpointHandler>();
        var first = SlackContext(SlackEvent("EvDuplicate"));
        var duplicate = SlackContext(SlackEvent("EvDuplicate"));

        await handler.HandleEventsAsync(
            first,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);
        await handler.HandleEventsAsync(
            duplicate,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, duplicate.Response.StatusCode);
        Assert.Equal(1, provider.GetRequiredService<IMessageIngressQueue>().GetHealthSnapshot().Accepted);
    }

    [Fact]
    public async Task DiscordAutocompleteHandlerProducesTheInitialChoicesInline() {
        using var provider = DiscordServices(capacity: 1).BuildServiceProvider();
        var dispatchCount = 0;
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.GetRequiredService<MessageRouter>().OnAutocomplete<DiscordInboundInteraction>(
            "search",
            async (_, cancellationToken) => {
                Interlocked.Increment(ref dispatchCount);
                entered.TrySetResult(true);
                await release.Task.WaitAsync(cancellationToken);
                return MessageHandlerResult.Respond(
                    DiscordInteractionAcknowledgement.Autocomplete(new[] {
                        DiscordAutocompleteChoice.FromString("Alpha", "alpha")
                    }));
            });
        const string json = """
            {"id":"100000000000000001","application_id":"100000000000000002","type":4,"token":"token","authorizing_integration_owners":{"1":"100000000000000003"},"user":{"id":"100000000000000003"},"data":{"name":"search","type":1,"options":[]}}
            """;
        var context = DiscordContext(json);
        var duplicate = DiscordContext(json);

        var handler = provider.GetRequiredService<DiscordHttpEndpointHandler>();
        var original = handler.HandleAsync(
            context,
            DiscordConfiguration(),
            TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);
        var replay = handler.HandleAsync(
            duplicate,
            DiscordConfiguration(),
            TestContext.Current.CancellationToken);
        Assert.False(replay.IsCompleted);
        release.TrySetResult(true);
        await Task.WhenAll(original, replay);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        using var body = JsonDocument.Parse(ResponseBody(context));
        Assert.Equal(8, body.RootElement.GetProperty("type").GetInt32());
        Assert.Equal("alpha", body.RootElement.GetProperty("data").GetProperty("choices")[0].GetProperty("value").GetString());
        Assert.Equal(StatusCodes.Status200OK, duplicate.Response.StatusCode);
        Assert.Equal(ResponseBody(context), ResponseBody(duplicate));
        Assert.Equal(1, Volatile.Read(ref dispatchCount));
        Assert.Equal(0, provider.GetRequiredService<IMessageIngressQueue>().GetHealthSnapshot().Accepted);
    }

    [Fact]
    public async Task DiscordSynchronousDispatchReturnsRetryableOverloadAndReleasesItsSlot() {
        using var provider = DiscordServices(capacity: 1, synchronousCapacity: 1).BuildServiceProvider();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var dispatchCount = 0;
        provider.GetRequiredService<MessageRouter>().OnAutocomplete<DiscordInboundInteraction>(
            "search",
            async (_, cancellationToken) => {
                Interlocked.Increment(ref dispatchCount);
                entered.TrySetResult(true);
                await release.Task.WaitAsync(cancellationToken);
                return MessageHandlerResult.Respond(DiscordInteractionAcknowledgement.EmptyAutocomplete());
            });
        static string RequestJson(string id) {
            const string template = """
                {"id":"__ID__","application_id":"100000000000000002","type":4,"token":"token","authorizing_integration_owners":{"0":"0"},"user":{"id":"100000000000000003"},"data":{"name":"search","type":1,"options":[]}}
                """;
            return template.Replace("__ID__", id, StringComparison.Ordinal);
        }
        var handler = provider.GetRequiredService<DiscordHttpEndpointHandler>();
        var first = DiscordContext(RequestJson("100000000000000011"));
        var overloaded = DiscordContext(RequestJson("100000000000000012"));
        var afterRelease = DiscordContext(RequestJson("100000000000000013"));

        var firstDispatch = handler.HandleAsync(
            first,
            new DiscordEndpointConfiguration(
                "application-a", DiscordPublicKey, "100000000000000002", "0"),
            TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await handler.HandleAsync(
            overloaded,
            new DiscordEndpointConfiguration(
                "application-a", DiscordPublicKey, "100000000000000002", "0"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, overloaded.Response.StatusCode);
        Assert.Equal("1", overloaded.Response.Headers.RetryAfter.ToString());
        release.TrySetResult(true);
        await firstDispatch;
        await handler.HandleAsync(
            afterRelease,
            new DiscordEndpointConfiguration(
                "application-a", DiscordPublicKey, "100000000000000002", "0"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, afterRelease.Response.StatusCode);
        Assert.Equal(2, Volatile.Read(ref dispatchCount));
    }

    [Fact]
    public async Task CanceledSynchronousDispatchReleasesItsCapacitySlot() {
        using var provider = DiscordServices(capacity: 1, synchronousCapacity: 1).BuildServiceProvider();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var call = 0;
        provider.GetRequiredService<MessageRouter>().OnAutocomplete<DiscordInboundInteraction>(
            "search",
            async (_, cancellationToken) => {
                if (Interlocked.Increment(ref call) == 1) {
                    entered.TrySetResult(true);
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                return MessageHandlerResult.Respond(DiscordInteractionAcknowledgement.EmptyAutocomplete());
            });
        const string firstJson = "{\"id\":\"100000000000000021\",\"application_id\":\"100000000000000022\",\"type\":4,\"token\":\"token\",\"authorizing_integration_owners\":{\"0\":\"0\"},\"user\":{\"id\":\"100000000000000023\"},\"data\":{\"name\":\"search\",\"type\":1,\"options\":[]}}";
        var handler = provider.GetRequiredService<DiscordHttpEndpointHandler>();
        var first = DiscordContext(firstJson);
        using var cancellation = new CancellationTokenSource();
        var firstDispatch = handler.HandleAsync(
            first,
            new DiscordEndpointConfiguration(
                "application-a", DiscordPublicKey, "100000000000000022", "0"),
            cancellation.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => firstDispatch);
        var retry = DiscordContext(firstJson);

        await handler.HandleAsync(
            retry,
            new DiscordEndpointConfiguration(
                "application-a", DiscordPublicKey, "100000000000000022", "0"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status200OK, retry.Response.StatusCode);
        Assert.Equal(2, Volatile.Read(ref call));
    }

    [Fact]
    public async Task ProviderEndpointsRejectOversizedContentTypeMetadata() {
        using var slackProvider = SlackServices(capacity: 1).BuildServiceProvider();
        using var discordProvider = DiscordServices(capacity: 1).BuildServiceProvider();
        var slack = SlackContext(SlackEvent("Ev1"));
        var discord = DiscordContext("{\"type\":1}");
        slack.Request.ContentType = "application/json;" + new string('a', 300);
        discord.Request.ContentType = "application/json;" + new string('a', 300);

        await slackProvider.GetRequiredService<SlackHttpEndpointHandler>().HandleEventsAsync(
            slack,
            SlackConfiguration(),
            TestContext.Current.CancellationToken);
        await discordProvider.GetRequiredService<DiscordHttpEndpointHandler>().HandleAsync(
            discord,
            DiscordConfiguration(),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, slack.Response.StatusCode);
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, discord.Response.StatusCode);
    }

    [Fact]
    public void ProviderConfigurationAndDependencySurfacesDoNotExposeSigningMaterialOrCrossLoadAdapters() {
        var slack = SlackConfiguration();
        var discord = DiscordConfiguration();
        using var slackProvider = SlackServices(capacity: 1).BuildServiceProvider();
        using var discordProvider = DiscordServices(capacity: 1).BuildServiceProvider();

        Assert.DoesNotContain(SlackSecret, JsonSerializer.Serialize(slack), StringComparison.Ordinal);
        Assert.DoesNotContain(SlackSecret, slack.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(DiscordPublicKey, JsonSerializer.Serialize(discord), StringComparison.OrdinalIgnoreCase);
        Assert.Null(slackProvider.GetService<DiscordHttpEndpointHandler>());
        Assert.Null(discordProvider.GetService<SlackHttpEndpointHandler>());
    }

    private static ServiceCollection SlackServices(int capacity) {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(SlackReceivedAt));
        services.AddMessageXHostingAspNetCore(options => options.QueueCapacity = capacity);
        services.AddMessageXSlackAspNetCore();
        return services;
    }

    private static ServiceCollection DiscordServices(int capacity, int? synchronousCapacity = null) {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(DiscordReceivedAt));
        services.AddMessageXHostingAspNetCore(options => {
            options.QueueCapacity = capacity;
            if (synchronousCapacity.HasValue) {
                options.SynchronousDispatchCapacity = synchronousCapacity.Value;
            }
        });
        services.AddMessageXDiscordAspNetCore();
        return services;
    }

    private static DefaultHttpContext SlackContext(string json) {
        var body = Encoding.UTF8.GetBytes(json);
        var context = Context(body, "application/json; charset=utf-8");
        context.Request.Headers["X-Slack-Request-Timestamp"] = SlackTimestamp;
        context.Request.Headers["X-Slack-Signature"] = SignSlack(json);
        return context;
    }

    private static DefaultHttpContext DiscordContext(string json) {
        var body = Encoding.UTF8.GetBytes(json);
        var context = Context(body, "application/json; charset=utf-8");
        context.Request.Headers["X-Signature-Timestamp"] = DiscordTimestamp;
        context.Request.Headers["X-Signature-Ed25519"] = SignDiscord(json);
        return context;
    }

    private static DefaultHttpContext Context(byte[] body, string contentType) {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "safe-endpoint-test";
        context.Request.ContentType = contentType;
        context.Request.ContentLength = body.Length;
        context.Request.Body = new MemoryStream(body);
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static byte[] ResponseBody(DefaultHttpContext context) =>
        ((MemoryStream)context.Response.Body).ToArray();

    private static SlackEndpointConfiguration SlackConfiguration() =>
        new("workspace-a", SlackSecret, "A1", "T1");

    private static DiscordEndpointConfiguration DiscordConfiguration() =>
        new(
            "application-a",
            DiscordPublicKey,
            "100000000000000002",
            "100000000000000003");

    private static string SlackEvent(string eventId) =>
        $"{{\"type\":\"event_callback\",\"api_app_id\":\"A1\",\"team_id\":\"T1\",\"event_id\":\"{eventId}\",\"event\":{{\"type\":\"message\",\"user\":\"U1\",\"channel\":\"C1\",\"ts\":\"1787416799.1\"}}}}";

    private static string SignSlack(string body) {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var prefix = Encoding.UTF8.GetBytes($"v0:{SlackTimestamp}:");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SlackSecret));
        hmac.TransformBlock(prefix, 0, prefix.Length, null, 0);
        hmac.TransformFinalBlock(bodyBytes, 0, bodyBytes.Length);
        return "v0=" + string.Concat(hmac.Hash!.Select(value => value.ToString("x2")));
    }

    private static string SignDiscord(string body) {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var timestampBytes = Encoding.ASCII.GetBytes(DiscordTimestamp);
        var signer = new Ed25519Signer();
        signer.Init(true, DiscordPrivateKey);
        signer.BlockUpdate(timestampBytes, 0, timestampBytes.Length);
        signer.BlockUpdate(bodyBytes, 0, bodyBytes.Length);
        return Convert.ToHexString(signer.GenerateSignature());
    }

    private sealed class FixedTimeProvider : TimeProvider {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
